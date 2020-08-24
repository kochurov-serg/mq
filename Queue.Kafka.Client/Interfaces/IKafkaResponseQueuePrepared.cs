using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Queue.Kafka.Client.Interfaces
{
	/// <summary>
	/// Store correlationId response
	/// </summary>
	public interface IKafkaResponseQueuePrepared
	{
		Task<HttpResponseMessage> Prepare(string correlationId, CancellationToken token);
	}
}