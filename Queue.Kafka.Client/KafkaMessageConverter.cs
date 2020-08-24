using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Kafka.Client.Interfaces;
using Queue.Kafka.Core;

namespace Queue.Kafka.Client
{
	/// <inheritdoc />
	public class KafkaMessageConverter: IKafkaMessageConverter
	{
		private readonly ILogger<KafkaMessageConverter> _log;

		public KafkaMessageConverter(ILogger<KafkaMessageConverter> log)
		{
			_log = log;
		}

		public void AddHeader(Headers headers, KeyValuePair<string, IEnumerable<string>> header) => headers.Add(header.Key.ToLowerInvariant(), KafkaExtensions.HeaderValue(header.Value));

		public void AddHeader(Headers headers, string key, string value) => headers.Add(key.ToLowerInvariant(), KafkaExtensions.HeaderValue(value));

		public void AddHeaders(Headers headers, HttpHeaders httpHeaders)
		{
			foreach (var header in httpHeaders)
			{
				AddHeader(headers, header);
			}
		}

		public async Task<Message<byte[], byte[]>> Convert(HttpRequestMessage request)
		{
			_log.LogTrace("Create kafka message");

			var content = request.Content != null
				? request.Content.ReadAsByteArrayAsync()
				: Task.FromResult(new byte[0]);

			var message = new Message<byte[], byte[]>
			{
				Key = null,
				Value = await content,
				Timestamp = new Timestamp(DateTimeOffset.Now.ToUnixTimeMilliseconds(), TimestampType.CreateTime),
				Headers = new Headers()
			};

			_log.LogTrace($"Add headers {QueueHeaders.Uri}:{request.RequestUri.AbsoluteUri}, {QueueHeaders.Method}:{request.Method.Method}");
			AddHeader(message.Headers, QueueHeaders.Uri, request.RequestUri.AbsoluteUri);
			AddHeader(message.Headers, QueueHeaders.Method, request.Method.Method);

			if (request.Headers.TryGetValues(KafkaQueueConsts.KafkaKey, out var key))
			{
				var firstKey = key.FirstOrDefault();
				if (string.IsNullOrWhiteSpace(firstKey))
					throw new ArgumentException($"If header {KafkaQueueConsts.KafkaKey} set, then header cannot be null or empty");
				_log.LogTrace($"Add key", firstKey);

				message.Key = KafkaExtensions.HeaderValue(firstKey);
			}

			AddHeaders(message.Headers, request.Headers);

			if (request.Content != null)
				AddHeaders(message.Headers, request.Content.Headers);

			return message;
		}
	}
}