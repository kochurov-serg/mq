using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using Queue.Server.Abstractions;

namespace Queue.Kafka.Server
{

	public class KafkaConverter : IKafkaConverter
	{
		private readonly KafkaServerOption _option;
		private readonly ILogger<KafkaServerOption> _log;

		public KafkaConverter(KafkaServerOption option, ILogger<KafkaServerOption> log)
		{
			_option = option;
			_log = log;
		}

		public string GetValue(byte[] headerValue) => Encoding.UTF8.GetString(headerValue);

		public Task<HttpContext> Parse(ConsumeResult<byte[], byte[]> consumeResult, FeatureCollection features)
		{
			_log.LogTrace("kafka request to http context. Parse headers");

			var headers = consumeResult.Message.Headers.Aggregate(new Dictionary<string, string>(consumeResult.Message.Headers.Count),
				(dictionary, header) =>
				{
					dictionary.TryAdd(header.Key, GetValue(header.GetValueBytes()));

					return dictionary;
				});
				
			headers.TryGetValue(QueueHeaders.Uri, out var uri);
			_log.LogTrace($"request Headers: {headers.Aggregate(new StringBuilder(), (builder, pair) => builder.Append($"{pair.Key}:{pair.Value}"))}");
			if (string.IsNullOrWhiteSpace(uri))
			{
				_log.LogError($"Header {QueueHeaders.Uri} missing. Request declined");

				return null;
			}
				
			if (!headers.TryGetValue(QueueHeaders.Method, out var method))
			{
				method = HttpMethods.Get;
				_log.LogWarning($"Header {QueueHeaders.Method} missing. Default method {method}");
			}

			var requestUri = new Uri(uri);
			var query = requestUri.ParseQueryString();
			var body = consumeResult.Message.Value;
			_log.LogTrace($"Create HttpRequest {uri} {method}");
			var contentTypeHeader = HeaderNames.ContentType.ToLowerInvariant();

			if (!headers.TryGetValue(contentTypeHeader, out var contentType))
			{
				contentType = QueueConsts.DefaultContentType;

				if (body != null)
				{
					_log.LogWarning($"Header {contentTypeHeader} is missing. Set value to {contentType}");
				}
			}

			var request = new QueueRequest
			{
				Body = body != null ? new MemoryStream(body) : Stream.Null,
				ContentType = contentType ?? QueueConsts.DefaultContentType,
				IsHttps = false,
				Method = method,
				Protocol = requestUri.Scheme,
				QueryString = new QueryString(requestUri.Query),
				Query = new QueryCollection(query.AllKeys.GroupBy(x => x).ToDictionary(x => x.Key, x => new StringValues(query[x.Key]))),
				Path = requestUri.AbsolutePath,
				Host = new HostString(requestUri.Host),
				Scheme = requestUri.Scheme,
				PathBase = PathString.Empty
			};

			request.ContentLength = request.Body.Length;

			headers.TryGetValue(HeaderNames.Accept, out var acceptValues);
			var accept = acceptValues;
			headers.TryGetValue(QueueHeaders.ReplyId, out var replyTo);

			_log.LogTrace("Create default HttpResponse");
			var response = new QueueResponse
			{
				ContentType = string.IsNullOrWhiteSpace(accept) ? QueueConsts.DefaultContentType : accept,
				StatusCode = StatusCodes.Status200OK,
				Body = !_option.ForceResponseStream && replyTo == null ? Stream.Null : new MemoryStream()
			};

			var context = new QueueContext(features, response, request);
			_log.LogTrace("Parse sucessful");

			return Task.FromResult(context as HttpContext);
		}
	}
}