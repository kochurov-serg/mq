using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Queue.Server.Abstractions;

namespace Queue.Rabbit.Server.Interfaces
{
	public interface IQueueConverter<T>
	{
		Task<QueueContext> Parse(T args, IFeatureCollection features);
	}
}