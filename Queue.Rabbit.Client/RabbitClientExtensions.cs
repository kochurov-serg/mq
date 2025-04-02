using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Queue.Core.Intefaces;
using Queue.Rabbit.Client.Interfaces;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Interfaces;
using Queue.Rabbit.Core.Options;

namespace Queue.Rabbit.Client
{
	/// <summary>
	/// Rabbit client connection to server
	/// </summary>
	public static class RabbitClientExtensions
	{
		/// <summary>
		/// Регистрация rabbit клиента
		/// </summary>
		/// <param name="services"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static IServiceCollection AddRabbitClient(this IServiceCollection services, Func<IServiceProvider, RabbitClientOptions> optionProvider)
		{
			services.TryAddTransient(optionProvider);
			services.TryAddTransient<IBasicDeliverEventArgsValidator, BasicDeliverEventArgsValidator>();
			services.TryAddTransient<IRabbitMessageConverter, RabbitMessageConverter>();
			services.TryAddTransient<IQueueClient, RabbitQueueClient>();
			services.TryAddTransient<IRabbitQueueClient, RabbitQueueClient>();
			services.TryAddTransient<RabbitMessageHandler>();
			services.TryAddTransient<RabbitDelegatingHandler>();
			services.TryAddSingleton<IRabbitResponseQueuePrepared, RabbitResponseQueuePrepared>();
			services.TryAddSingleton<IRabbitConnectionFactory, DefaultRabbitConnectionFactory>();

			return services;
		}

		/// <summary>
		/// Configure rabbit request
		/// </summary>
		/// <param name="request">HttpRequesTMessage</param>
		/// <param name="option">Options</param>
		/// <returns></returns>
		public static HttpRequestMessage AddRabbitRequestOption(this HttpRequestMessage request, RabbitRequestOption option)
		{
			if (option != null)
				request.Options.Set(new HttpRequestOptionsKey<RabbitRequestOption>(RabbitRequestOption.RequestProperty), option);

			return request;
		}
	}
}