using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Queue.Kafka.Client.Interfaces;

namespace Queue.Kafka.Client
{
	/// <summary>
	/// Default producer builder
	/// </summary>
	public class DefaultKafkaQueueProducerBuilder : IKafkaQueueProducerBuilder
	{
		private const string Separator = ", ";
		private readonly KafkaOptions _options;
		private readonly ILogger<DefaultKafkaQueueProducerBuilder> _log;

		public DefaultKafkaQueueProducerBuilder(KafkaOptions options, ILogger<DefaultKafkaQueueProducerBuilder> log)
		{
			_options = options;
			_log = log;
		}

		public string ErrorMessage(Error error) => string.Join(Separator,
			error.IsLocalError ? "Local error" : "Broker error", error.Code,
			error.Reason);

		public IProducer<byte[], byte[]> CreateProducer()
		{
			var producer = new ProducerBuilder<byte[], byte[]>(_options.ProducerConfig)
				.SetErrorHandler((_, error) => _log.LogError(ErrorMessage(error)))
				.SetLogHandler((_, message) =>
					_log.LogTrace(
						$"Client instance: {message.Name}, facility: {message.Facility}, {message.Level} {message.Message}"))
				.SetStatisticsHandler((_, s) => _log.LogTrace(s)).Build();

			return producer;
		}
	}
}