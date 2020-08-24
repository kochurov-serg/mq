using Confluent.Kafka;

namespace Queue.Kafka.Client.Interfaces
{
	/// <summary>
	/// Producer builder
	/// </summary>
	public interface IKafkaQueueProducerBuilder
	{
		/// <summary>
		/// Create new prducer
		/// </summary>
		/// <returns></returns>
		IProducer<byte[], byte[]> CreateProducer();
	}
}