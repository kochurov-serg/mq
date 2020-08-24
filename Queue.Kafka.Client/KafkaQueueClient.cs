using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Kafka.Client.Interfaces;

namespace Queue.Kafka.Client
{
	public class KafkaQueueClient : IKafkaQueueClient
	{
		private readonly ILogger<KafkaQueueClient> _log;
		private readonly IKafkaMessageConverter _converter;
		private readonly IKafkaQueueProducerBuilder _producerBuilder;
		private readonly IKafkaResponseQueuePrepared _responseQueuePrepared;
		private readonly KafkaOptions _options;

		private static HttpResponseMessage SuccessResponse => new HttpResponseMessage(HttpStatusCode.OK);

		public KafkaQueueClient(ILogger<KafkaQueueClient> log, IKafkaMessageConverter converter, IKafkaQueueProducerBuilder producerBuilder, IKafkaResponseQueuePrepared responseQueuePrepared, KafkaOptions options)
		{
			_log = log;
			_converter = converter;
			_producerBuilder = producerBuilder;
			_responseQueuePrepared = responseQueuePrepared;
			_options = options;
		}

		public async Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			using (var producer = _producerBuilder.CreateProducer())
			{
				var correlationId = request.Headers.GetCorrelationHeader();
				Task<HttpResponseMessage> responseTask = null;

				if (correlationId != null)
				{
					_log.LogTrace("correlation header is set. sync request");
					responseTask = _responseQueuePrepared.Prepare(correlationId, cancellationToken);
					request.Headers.Add(QueueHeaders.ReplyTo, _options.ResponseTopic);
				}

				var message = await _converter.Convert(request);
				cancellationToken.ThrowIfCancellationRequested();

				_log.LogTrace($"Send message kafka to topic {request.RequestUri.Host}");
				var result = await producer.ProduceAsync(request.RequestUri.Host, message);
				var statusText = result.Status.ToString();
				_log.LogTrace(
					$"Message sended. Partition {result.Partition.Value}, Topic partition offset {result.TopicPartitionOffset.Offset.Value} status: {statusText}");

				if (result.Status == PersistenceStatus.Persisted)
				{
					return responseTask != null ? await responseTask : SuccessResponse;
				}

				return new HttpResponseMessage(HttpStatusCode.UnprocessableEntity) { ReasonPhrase = statusText };
			}
		}
	}
}
