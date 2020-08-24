using System.Net.Http;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Queue.Kafka.Client.Interfaces
{
	/// <summary>
	/// Convert HttpRequestMessage to Kafka Message
	/// </summary>
	public interface IKafkaMessageConverter
	{
		Task<Message<byte[], byte[]>> Convert(HttpRequestMessage request);
	}
}