using System;
using Microsoft.Extensions.DependencyInjection;

namespace Queue.Nats.Client.HttpClientBuilder
{
	public static class QueueNatsHttpClientBuilderExtensions
	{
		public static IHttpClientBuilder ConfigurePrimaryNatsMessageHandler(this IHttpClientBuilder builder, Func<IServiceProvider, NatsQueueClientOption> optionProvider)
		{
			builder.Services.AddNatsClient(optionProvider);
			return builder.ConfigurePrimaryHttpMessageHandler(provider =>
			{
				var handler = provider.GetRequiredService<NatsMessageHandler>();

				return handler;
			});
		}
		/// <summary>
		/// Require register AddRabbitClient.
		/// </summary>
		/// <param name="builder">IHttpClientBuilder replace send request to rabbitmq</param>
		/// <returns></returns>
		public static IHttpClientBuilder ConfigurePrimaryNatsMessageHandler(this IHttpClientBuilder builder) => builder.ConfigurePrimaryHttpMessageHandler(provider => provider.GetRequiredService<NatsMessageHandler>());
	}
}
