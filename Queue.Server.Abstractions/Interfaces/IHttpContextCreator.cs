using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Queue.Server.Abstractions.Interfaces
{
	public interface IHttpContextCreator
	{
		Task<HttpContext> Create(byte[] bytes, IFeatureCollection features, bool needResponseStream);
	}
}