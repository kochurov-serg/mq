using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;

namespace Queue.Kafka.Server
{
	/// <summary>
	/// Kafka response processed
	/// </summary>
	public interface IKafkaResponseProcessed
	{
		Task Handle(ConsumeResult<byte[], byte[]> message, HttpContext context);
	}
}