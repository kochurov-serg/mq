using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NATS.Client;

namespace Queue.Nats.Server.Interfaces
{
	public interface INatsConverter
	{
		Task<HttpContext> Parse(MsgHandlerEventArgs args, IFeatureCollection features);
	}
}