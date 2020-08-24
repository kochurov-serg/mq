using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Notification.Amqp.Server.Abstractions;
using RabbitMQ.Client.Events;

namespace Notification.Amqp.Server.RabbitMq
{
	/// <inheritdoc />
	public class RabbitAmqpConverter : IAmqpConverter<BasicDeliverEventArgs>
	{
		private readonly ILogger<RabbitAmqpConverter> _log;
		private readonly IBasicDeliverEventArgsValidator _validator;

		public RabbitAmqpConverter(ILogger<RabbitAmqpConverter> log, IBasicDeliverEventArgsValidator validator)
		{
			_log = log;
			_validator = validator;
		}

		public Task<AmqpContext> Parse(BasicDeliverEventArgs args, IFeatureCollection features)
		{
			try
			{
				_validator.Validate(args);
				var props = args.BasicProperties;
				var headers = args.BasicProperties.Headers;

				var method = headers.GetOrDefaultString(AmqpHeaders.Method, HttpMethods.Get);
				var accept = headers.GetOrDefaultString(HeaderNames.Accept,AmqpCommunication.DefaultContentType);
				var uri = headers.GetOrDefaultString(AmqpHeaders.Uri);
				var contentType = headers.GetOrDefaultString(HeaderNames.ContentType);

				var startIndex = uri.IndexOf('?');
				var queryString = new QueryString(startIndex == -1 ? string.Empty : new string(uri.SkipWhile(c => c != '?').ToArray()));
				var query = HttpUtility.ParseQueryString(queryString.Value ?? string.Empty);
				var host = props.Headers.GetOrDefaultString(HeaderNames.Host, null);

				var request = new AmqpHttpRequest
				{
					Body = !(method == HttpMethods.Get || method == HttpMethods.Delete) ? new MemoryStream(args.Body, false) : null,
					ContentLength = args.Body.LongLength,
					ContentType = contentType,
					IsHttps = false,
					Method = method,
					Protocol = props.ProtocolClassName,
					QueryString = queryString,
					Query = new QueryCollection(query.AllKeys.ToDictionary(x => x, x => new StringValues(query[x]))),
					Path = uri,
					Host = host != null ? new HostString(host) : new HostString(string.Empty),
					Scheme = AmqpCommunication.ProtocolName,
					PathBase = PathString.Empty,
				};

				foreach (var header in headers)
				{
					request.Headers.TryAdd(header.Key, Encoding.UTF8.GetString((byte[])header.Value));
				}

				var response = new AmqpHttpResponse
				{
					ContentType = accept,
					StatusCode = StatusCodes.Status200OK,
				};

				var context = new AmqpContext(features)
				{
				};

				request.Initialize(context);
				response.Initialize(context);

				// Удалить после тестирования
				context.Request.Headers["Authorization"] = "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjMyZTA0MDJjMGJjNDcxMzllOTZjODU2YjAxYmQyY2M5IiwidHlwIjoiSldUIn0.eyJuYmYiOjE1NDg0NzY2NjcsImV4cCI6MTYxMTU0ODY2NywiaXNzIjoiaHR0cDovL2F1dGgtZGV2Lm1vdmlzdGEucnUiLCJhdWQiOlsiaHR0cDovL2F1dGgtZGV2Lm1vdmlzdGEucnUvcmVzb3VyY2VzIiwiTm90aWZpY2F0aW9uIl0sImNsaWVudF9pZCI6InN3YWdnZXIiLCJzY29wZSI6WyJOb3RpZmljYXRpb24uRnVsbEFjY2VzcyJdfQ.yN5xJMEKaGUs5Ya78S5fh7S3VnL2fKT5XSud7sKAdjH7zielWHvm3dsudiv_u2WV-1QQI-IKMVVKt1BYLMA8Eoru6eIGRlaahlLzGpS1S3MpulRa24muzjKtbGVG261h3jynOabPiSYMg_JcMbbgMwVZ3CMiGn1-YiaXVwdL3nMdeJ8UIHl7VaXSpcw0z3X84G4Hyb2CQ0-HUahueh5N1l9BJpyr4bBXIGnZBpOWeP6IWAUfXsuKpM_lYW76J5Hbb50-lsfMgJ3s5GXd4ekL27gPZ37eDpq3FA10AxWrJNag4hq2B0QJtoxYw3Sbzy-tJG_wTLFlT8GX-RNfBLLgpA";

				return Task.FromResult(context);
			}
			catch (Exception e)
			{
				_log.LogError(e, "Ошибка формирования httpContext");

				return Task.FromResult(default(AmqpContext));
			}
		}
	}
}