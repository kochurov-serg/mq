using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Queue.Client;
using Queue.Client.Interfaces;
using Queue.Kafka.Client.Interfaces;

namespace Queue.Kafka.Client
{
	public static class KafkaClientQueueExtensions
	{
		public static IServiceCollection AddKafkaClient(this IServiceCollection services, Func<IServiceProvider, KafkaOptions> configureOptions)
		{
			services.TryAddTransient<IKafkaQueueClient, KafkaQueueClient>();
			services.TryAddTransient<KafkaMessageHandler>();
			services.TryAddTransient<IHttpResponseParser, HttpResponseParser>();
			services.TryAddTransient<IKafkaMessageConverter, KafkaHttpRequestConverter>();
			services.TryAddTransient<IKafkaQueueProducerBuilder, DefaultKafkaQueueProducerBuilder>();
			services.TryAddSingleton<IKafkaResponseQueuePrepared, KafkaResponseQueuePrepared>();
			services.TryAddTransient(configureOptions);

			return services;
		}



	}
}
