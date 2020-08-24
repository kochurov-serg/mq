using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Server.Abstractions;

namespace Queue.Kafka.Server
{
	public class KafkaHttpResponseProcessed : IKafkaResponseProcessed
	{
		private readonly ILogger<KafkaResponseProcessed> _log;
		private readonly KafkaServerOption _option;
		private readonly IResponseConverter _responseConverter;

		public KafkaHttpResponseProcessed(ILogger<KafkaResponseProcessed> log, KafkaServerOption option, IResponseConverter responseConverter)
		{
			_log = log;
			_option = option;
			_responseConverter = responseConverter;
		}

		/// <inheritdoc />
		public async Task Handle(ConsumeResult<byte[], byte[]> message, HttpContext context)
		{
			context.Request.Headers.TryGetValue(QueueHeaders.ReplyTo, out var replyTo);
			context.Request.Headers.TryGetValue(QueueHeaders.CorrelationId, out var correlationId);

			if (string.IsNullOrWhiteSpace(replyTo))
			{
				_log.LogTrace($"header {QueueHeaders.ReplyTo} not set. Response not be sent.");
				return;
			}

			context.Response.Headers.TryAdd(QueueHeaders.CorrelationId, correlationId);
			var bodyTask = _responseConverter.Convert(context.Response);

			var response = new Message<byte[], byte[]>
			{
				Timestamp = new Timestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), TimestampType.CreateTime),
				Value = await bodyTask
			};

			using (var producer = new ProducerBuilder<byte[], byte[]>(_option.ProducerConfig).Build())
			{
				var result = await producer.ProduceAsync(replyTo, response);
				_log.LogTrace($"Response sent to {replyTo} status: {result.Status}");
			}
		}
	}
}
