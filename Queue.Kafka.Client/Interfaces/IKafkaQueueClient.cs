using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Queue.Kafka.Client.Interfaces
{
	/// <summary>
	/// Kafka client 
	/// </summary>
	public interface IKafkaQueueClient
	{
		Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken cancellationToken);
	}
}