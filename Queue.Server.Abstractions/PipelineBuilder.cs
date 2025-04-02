using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Queue.Server.Abstractions.Interfaces;

namespace Queue.Server.Abstractions
{
	/// <inheritdoc />
	public class PipelineBuilder : IPipelineBuilder
	{
		private readonly IServiceScopeFactory _scopeFactory;

		/// <inheritdoc />
		public PipelineBuilder(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory;
		}

		/// <inheritdoc />
		public async Task Build<T>(IApplicationBuilder applicationBuilder, CancellationToken cancellationToken) where T : IQueueServer
		{
			var process = applicationBuilder.Build();

			async Task Delegate(HttpContext context)
			{
				if (context == null) throw new ArgumentNullException(nameof(context));

                using var scope = _scopeFactory.CreateScope();

                var accessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
                if (accessor != null)
                    accessor.HttpContext = context;

                context.RequestServices = scope.ServiceProvider;

                await process(context).ConfigureAwait(false);
            }

			var serviceProvider = _scopeFactory.CreateScope().ServiceProvider;
			var server = serviceProvider.GetRequiredService<T>();
			await server.Start(Delegate, cancellationToken);
		}
	}
}
