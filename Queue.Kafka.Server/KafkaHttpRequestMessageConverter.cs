using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Server.Abstractions.Interfaces;

namespace Queue.Kafka.Server
{
	public class KafkaHttpRequestMessageConverter : IKafkaConverter
	{
		private readonly ILogger<KafkaHttpRequestMessageConverter> _log;
		private readonly IHttpContextCreator _creator;
		private readonly KafkaServerOption _options;

		public KafkaHttpRequestMessageConverter(ILogger<KafkaHttpRequestMessageConverter> log, IHttpContextCreator creator, KafkaServerOption options)
		{
			_log = log;
			_creator = creator;
			_options = options;
		}

		/// <inheritdoc />
		public async Task<HttpContext> Parse(ConsumeResult<byte[], byte[]> request, FeatureCollection features)
		{
			var context = await _creator.Create(request.Message.Value, features, !_options.ForceResponseStream);

			if (context == null)
				return null;

			if (context.Request.Headers.TryGetValue(QueueHeaders.ReplyTo, out var replyValues))
			{
				if (string.IsNullOrWhiteSpace(replyValues))
					_log.LogError($"Request to uri {context.Request.GetDisplayUrl()} header {QueueHeaders.ReplyTo} is set but value cannot be empty. Response not be sent");
				else
				{
					_log.LogTrace("create memory stream response");
					if (!(context.Response.Body is MemoryStream))
						context.Response.Body = new MemoryStream();
				}
			}

			return context;
		}
	}
}
