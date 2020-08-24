using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Interfaces;
using Queue.Rabbit.Server.Interfaces;
using Queue.Server.Abstractions;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server
{
	/// <inheritdoc />
	public class RabbitConverter : IQueueConverter<BasicDeliverEventArgs>
	{
		private readonly ILogger<RabbitConverter> _log;
		private readonly IBasicDeliverEventArgsValidator _validator;
		private readonly RabbitServerOptions _options;

		public RabbitConverter(ILogger<RabbitConverter> log, IBasicDeliverEventArgsValidator validator, RabbitServerOptions options)
		{
			_log = log;
			_validator = validator;
			_options = options;
		}

		public Task<QueueContext> Parse(BasicDeliverEventArgs args, IFeatureCollection features)
		{
			try
			{
				_log.LogTrace("Validate request");
				_validator.Validate(args);
				var props = args.BasicProperties;
				var headers = args.BasicProperties.Headers.ToDictionary(x => x.Key.ToLower(CultureInfo.InvariantCulture), pair => pair.Value);
				_log.LogTrace("Parsing standart headers method, accept, uri, content-type e.t.c");
				var method = headers.GetOrDefaultString(QueueHeaders.Method, HttpMethods.Get);
				var accept = headers.GetOrDefaultString(HeaderNames.Accept.ToLowerInvariant(), QueueConsts.DefaultContentType);
				var uriHeader = headers.GetOrDefaultString(QueueHeaders.Uri);
				if (!Uri.TryCreate(uriHeader, UriKind.RelativeOrAbsolute, out var uri))
					throw new ArgumentException($"Header {QueueHeaders.Uri} must be valid uri absolute or relative");

				var relativeUri = uri.IsAbsoluteUri ? QueueNameExtensions.GetQueue(uri) : uriHeader;

				if (relativeUri[0] != '/')
					relativeUri = "/" + relativeUri;

				var contentType = headers.GetOrDefaultString(HeaderNames.ContentType.ToLowerInvariant(), QueueConsts.DefaultContentType);

				_log.LogTrace("Parse query parameters");
				var startIndex = uriHeader.IndexOf('?');
				var queryString = new QueryString(startIndex == -1 ? string.Empty : uriHeader.Substring(startIndex));
				var query = HttpUtility.ParseQueryString(queryString.Value ?? string.Empty);
				var host = headers.GetOrDefaultString(HeaderNames.Host.ToLowerInvariant(), _options.Queue.Uri.Host);

				var emptyBody = method == HttpMethods.Get || method == HttpMethods.Delete;

				if (emptyBody && args.Body.Length != 0)
				{
					_log.LogWarning($"{args.ConsumerTag}, uri: {relativeUri} method not available body. But body is set");
				}

				MemoryMarshal.TryGetArray(args.Body, out ArraySegment<byte> array);
				var request = new QueueRequest
				{
					Body = !emptyBody ? new MemoryStream(array.Array, 0, array.Count, false) : Stream.Null,
					ContentLength = array.Count,
					ContentType = contentType,
					IsHttps = false,
					Method = method,
					Protocol = props.ProtocolClassName,
					QueryString = queryString,
					Query = new QueryCollection(query.AllKeys.ToDictionary(x => x, x => new StringValues(query[x]))),
					Path = relativeUri,
					Host = host != null ? new HostString(host) : new HostString(string.Empty),
					Scheme = RabbitConsts.Schema,
					PathBase = PathString.Empty,
				};

				_log.LogTrace("Filling in the request headers");
				foreach (var header in headers)
				{
					if (header.Value is List<object> list)
					{

					}
					else
					{
						request.Headers.TryAdd(header.Key, Encoding.UTF8.GetString((byte[])header.Value));
					}
				}

				_log.LogTrace($"Create default response");
				var response = new QueueResponse
				{
					ContentType = accept,
					StatusCode = StatusCodes.Status200OK,
					Body = !_options.ForceResponseStream && args.BasicProperties.ReplyTo == null ? Stream.Null : new MemoryStream()
				};

				_log.LogTrace($"Create queue context");
				var context = new QueueContext(features, response, request);

				return Task.FromResult(context);
			}
			catch (Exception e)
			{
				_log.LogError(e, "Error create Context");

				return Task.FromResult(default(QueueContext));
			}
		}
	}
}