using System;
using Confluent.Kafka;

namespace Queue.Kafka.Client
{
	/// <summary>
	/// Configuration kafka client
	/// </summary>
	public class KafkaOptions
	{
		/// <summary>
		/// Unique client postfix, need for create personalization queue. Property is null or empty if ConsumerConfig.ClientId unique or client not member cluster
		/// </summary>
		public string ClientUnique { get; set; }
		public ProducerConfig ProducerConfig { get; set; }

		public TimeSpan ResponseWait { get; set; } = TimeSpan.FromMinutes(10);

		public ConsumerConfig ConsumerConfig { get; set; }

		public string ResponseTopic => string.Join("-",ConsumerConfig.ClientId, ClientUnique);
	}
}