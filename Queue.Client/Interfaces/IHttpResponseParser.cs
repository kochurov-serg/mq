using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Queue.Client.Interfaces
{
	/// <summary>
	/// HttpResponseMessage parser
	/// </summary>
	public interface IHttpResponseParser
	{
		Task<HttpResponseMessage> Parse(byte[] bytes, CancellationToken token);
	}
}