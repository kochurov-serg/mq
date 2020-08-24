using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client
{
	/// <summary>
	/// Create queue and wait response
	/// </summary>
	public interface IRabbitResponseQueuePrepared
	{
		/// <summary>
		/// Create queue and wait response
		/// </summary>
		/// <param name="correlationId"></param>
		/// <param name="factory"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		Task<HttpResponseMessage> Prepare(string correlationId, ConnectionFactory factory, CancellationToken token);
	}
}