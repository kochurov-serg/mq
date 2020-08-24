using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Interfaces;
using Queue.Rabbit.Server.Interfaces;
using Queue.Server.Abstractions;
using Queue.Server.Abstractions.Interfaces;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server.Extensions
{
	/// <summary>
	/// Rabbit server extensions
	/// </summary>
	public static class RabbitServerExtensions
	{
		/// <summary>
		/// Register Rabbit server
		/// </summary>
		/// <param name="services">service collection</param>
		/// <param name="options">server options</param>
		/// <returns></returns>
		public static IServiceCollection AddRabbitServer(this IServiceCollection services, RabbitServerOptions options)
		{
			if (options.Connection == null)
				throw new ArgumentNullException(string.Join(".", nameof(options), nameof(options.Connection)), "Required");

			services.TryAddTransient(provider => options);
			services.TryAddTransient(provider => options.Connection);
			services.TryAddScoped<IRabbitCommunicationServer, RabbitServerCommunication>();
			services.TryAddTransient<IQueueConverter<BasicDeliverEventArgs>, RabbitConverter>();
			services.TryAddTransient<IRabbitDelayConfig, RabbitDelayConfig>();
			services.TryAddSingleton<IPipelineBuilder, PipelineBuilder>();
			services.TryAddTransient<IBasicDeliverEventArgsValidator, BasicDeliverEventArgsValidator>();
			services.TryAddTransient<IRabbitmqConnection, RabbitmqConnection>();
			services.TryAddScoped<IRabbitQueueServer, RabbitQueueServer>();
			services.TryAddScoped<IRabbitResponseProcessed, RabbitResponseProcessed>();

			return services;
		}

		public static async Task UseRabbitServer(this IApplicationBuilder app, CancellationToken cancellationToken)
		{
			await app.UseServer<IRabbitQueueServer>(cancellationToken);
		}
	}
}
