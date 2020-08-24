
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Queue.Nats.Client.Interfaces;
using Queue.Nats.Core;

namespace Queue.Nats.Client
{
	public static class QueueNatsClientExtensions
	{
		public static IServiceCollection AddNatsClient(this IServiceCollection services, Func<IServiceProvider, NatsQueueClientOption> optionProvider)
		{
			services.TryAddSingleton<INatsQueueConnection, DefaultNatsQueueConnection>();
			services.TryAddTransient<INatsQueueClient, NatsQueueClient>();
			services.TryAddTransient<NatsMessageHandler>();
			services.TryAddTransient(optionProvider);

			return services;
		}
	}
}
