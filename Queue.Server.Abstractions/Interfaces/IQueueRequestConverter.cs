using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Queue.Server.Abstractions.Interfaces
{
	/// <summary>
	/// Transform Http request message to Queue request
	/// </summary>
	public interface IQueueRequestConverter
	{
		/// <summary>
		/// Transform bytes to http request
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		Task<QueueRequest> Convert(byte[] bytes);

		Task<QueueRequest> Convert(Stream stream);
		Task<QueueRequest> Convert(HttpRequestMessage message);
	}
}