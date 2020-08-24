using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Queue.Kafka.Client.Interfaces;
using Queue.Kafka.Core;

namespace Queue.Kafka.Client
{
	public class KafkaHttpRequestConverter : IKafkaMessageConverter
	{
		private readonly ILogger<KafkaHttpRequestConverter> _log;

		public KafkaHttpRequestConverter(ILogger<KafkaHttpRequestConverter> log)
		{
			_log = log;
		}

		/// <inheritdoc />
		public async Task<Message<byte[], byte[]>> Convert(HttpRequestMessage request)
		{
			_log.LogTrace("create full HttpRequestMessage to kafka");
			var content = new HttpMessageContent(request);
			var bytesTask = content.ReadAsByteArrayAsync();

			var message = new Message<byte[], byte[]>
			{
				Key = null,
				Value = await bytesTask,
				Timestamp = new Timestamp(DateTimeOffset.Now.ToUnixTimeMilliseconds(), TimestampType.CreateTime),
			};

			if (request.Headers.TryGetValues(KafkaQueueConsts.KafkaKey, out var key))
			{
				var firstKey = key.FirstOrDefault();
				if (string.IsNullOrWhiteSpace(firstKey))
					throw new ArgumentException($"If header {KafkaQueueConsts.KafkaKey} set, then header cannot be null or empty");
				_log.LogTrace($"Add key", firstKey);

				message.Key = KafkaExtensions.HeaderValue(firstKey);
			}

			return message;
		}
	}
}
