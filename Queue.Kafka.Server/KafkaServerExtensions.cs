using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Queue.Server.Abstractions;
using Queue.Server.Abstractions.Interfaces;

namespace Queue.Kafka.Server
{
	public static class KafkaServerExtensions
	{
		public static IServiceCollection AddKafkaServer(this IServiceCollection services, Func<IServiceProvider, KafkaServerOption> optionProvider)
		{
			services.TryAddTransient<IKafkaQueueServer, KafkaQueueServer>();
			services.TryAddTransient<IKafkaConverter, KafkaHttpRequestMessageConverter>();
			services.TryAddTransient<IKafkaResponseProcessed, KafkaHttpResponseProcessed>();
			services.TryAddTransient<IHttpContextCreator, HttpContextCreator>();
			services.TryAddTransient<IQueueRequestConverter, QueueRequestConverter>();
			services.TryAddTransient<IResponseConverter, ResponseConverter>();
			services.TryAddTransient(optionProvider);
			services.TryAddTransient<IPipelineBuilder, PipelineBuilder>();

			return services;
		}

		public static async Task UseKafkaServer(this IApplicationBuilder app, CancellationToken cancellationToken)
		{
			await app.UseServer<IKafkaQueueServer>(cancellationToken);
		}
	}
}
