using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Queue.Nats.Core;
using Queue.Nats.Server.Interfaces;
using Queue.Server.Abstractions;
using Queue.Server.Abstractions.Interfaces;

namespace Queue.Nats.Server
{
	public static class NatsServerExtensions
	{
		public static IServiceCollection AddNatsServer(this IServiceCollection services, Func<IServiceProvider, NatsServerOption> optionProvider)
		{
			services.TryAddTransient<INatsQueueServer, NatsQueueServer>();
			services.TryAddTransient<IQueueRequestConverter, QueueRequestConverter>();
			services.TryAddTransient<INatsConverter, NatsConverter>();
			services.TryAddTransient<IHttpContextCreator, HttpContextCreator>();
			services.TryAddTransient(optionProvider);
			services.TryAddTransient<INatsQueueConnection, DefaultNatsQueueConnection>();
			services.TryAddSingleton<IPipelineBuilder, PipelineBuilder>();
			services.TryAddTransient<INatsResponseProcessed, NatsResponseProcessed>();

			return services;
		}

		public static async Task UseNatsServer(this IApplicationBuilder app, CancellationToken cancellationToken)
		{
			await app.UseServer<INatsQueueServer>(cancellationToken).ConfigureAwait(false);
		}
	}
}
