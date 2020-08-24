using System;
using Microsoft.Extensions.DependencyInjection;

namespace Queue.Kafka.Client.HttpClientBuilder
{
	public static class QueueKafkaHttpClientBuilderExtensions
	{
		public static IHttpClientBuilder ConfigurePrimaryKafkaMessageHandler(this IHttpClientBuilder builder, Func<IServiceProvider, KafkaOptions> optionProvider)
		{
			builder.Services.AddKafkaClient(optionProvider);
			return builder.ConfigurePrimaryHttpMessageHandler(provider =>
			{
				var handler = provider.GetRequiredService<KafkaMessageHandler>();

				return handler;
			});
		}
		/// <summary>
		/// Require register AddRabbitClient.
		/// </summary>
		/// <param name="builder">IHttpClientBuilder replace send request to rabbitmq</param>
		/// <returns></returns>
		public static IHttpClientBuilder ConfigurePrimaryNatsMessageHandler(this IHttpClientBuilder builder) => builder.ConfigurePrimaryHttpMessageHandler(provider => provider.GetRequiredService<KafkaMessageHandler>());
	}
}
