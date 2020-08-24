using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NATS.Client;

namespace Queue.Nats.Server
{
	public interface INatsResponseProcessed
	{
		Task ProcessResponse(MsgHandlerEventArgs args, HttpContext context, IConnection connection);
	}
}