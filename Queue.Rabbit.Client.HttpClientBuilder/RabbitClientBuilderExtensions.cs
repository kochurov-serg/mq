using System;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client.HttpClientBuilder
{
	public static class RabbitClientBuilderExtensions
	{
		public static IHttpClientBuilder ConfigurePrimaryRabbitMessageHandler(this IHttpClientBuilder builder, ConnectionFactory factory, Func<IServiceProvider, RabbitClientOptions> optionProvider)
		{
			builder.Services.AddRabbitClient(optionProvider);
			return builder.ConfigurePrimaryHttpMessageHandler(provider =>
			{
				var handler = provider.GetRequiredService<RabbitMessageHandler>();
				handler.Factory = factory;

				return handler;
			});
		}
		/// <summary>
		/// Require register AddRabbitClient.
		/// </summary>
		/// <param name="builder">IHttpClientBuilder replace send request to rabbitmq</param>
		/// <returns></returns>
		public static IHttpClientBuilder ConfigurePrimaryRabbitMessageHandler(this IHttpClientBuilder builder) => builder.ConfigurePrimaryHttpMessageHandler(provider => provider.GetRequiredService<RabbitMessageHandler>());
	}
}