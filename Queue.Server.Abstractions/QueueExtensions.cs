using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Queue.Server.Abstractions.Interfaces;

namespace Queue.Server.Abstractions
{
	public static class QueueExtensions
	{
		public static async Task UseServer<T>(this IApplicationBuilder app, CancellationToken cancellationToken) where T : IQueueServer
		{
			var serviceProvider = app.ApplicationServices.CreateScope().ServiceProvider;
			var pipeline = serviceProvider.GetRequiredService<IPipelineBuilder>();
			await pipeline.Build<T>(app, cancellationToken).ConfigureAwait(false);
		}
	}
}
