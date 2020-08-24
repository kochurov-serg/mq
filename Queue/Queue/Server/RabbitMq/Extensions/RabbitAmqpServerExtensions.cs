using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notification.Amqp.Client.Abstractions;
using Notification.Amqp.Client.Abstractions.Interfaces;
using Notification.Amqp.Client.RabbitMq;
using Notification.Amqp.Server.Abstractions;
using Notification.Amqp.Server.Abstractions.Interfaces;
using RabbitMQ.Client.Events;

namespace Notification.Amqp.Server.RabbitMq
{
	public static class RabbitAmqpServerExtensions
	{
		/// <summary>
		/// Регистрация Rabbit сервера
		/// </summary>
		/// <param name="services"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static IServiceCollection AddRabbitServer(this IServiceCollection services, AmqpServerOptions options)
		{
			services.AddTransient(provider => options);
			services.TryAddTransient(provider => options.Connection);
			services.TryAddTransient<IRabbitAmqpCommunication, RabbitAmqpCommunication>();
			services.TryAddTransient(provider => options.Connection.ConnectionFactory);
			services.TryAddTransient<IAmqpConverter<BasicDeliverEventArgs>, RabbitAmqpConverter>();
			services.TryAddTransient<IVirtualAmqpServer, VirtualAmqpServer>();
			services.TryAddTransient<IBasicDeliverEventArgsValidator, BasicDeliverEventArgsValidator>();
			services.AddSingleton<IAmqpServer, RabbitAmqpServer>();

			return services;
		}

		public static void UseRabbitServer(this IApplicationBuilder app)
		{
			var amqpServer = app.ApplicationServices.GetRequiredService<IVirtualAmqpServer>();
			amqpServer.StartAsync(app, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}

	/// <summary>
	/// Подключения Rabbit amqp client к серверу
	/// </summary>
	public static class RabbitAmqpClientExtensions
	{
		/// <summary>
		/// Регистрация rabbit клиента
		/// </summary>
		/// <param name="services"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static IServiceCollection AddRabbitClient(this IServiceCollection services, AmqpClientOptions options)
		{
			services.TryAddTransient(provider => options);
			services.TryAddTransient(provider => options.Connection);
			services.TryAddTransient<IAmqpConverter<BasicDeliverEventArgs>, RabbitAmqpConverter>();
			services.TryAddTransient<IHttpRequestPropertiesParser, HttpRequestPropertiesParser>();
			services.TryAddTransient<IRabbitAmqpCommunication, RabbitAmqpCommunication>();
			services.TryAddTransient<IBasicDeliverEventArgsValidator, BasicDeliverEventArgsValidator>();
			services.TryAddSingleton<IVirtualAmqpClient, VirtualAmqpClient>();
			services.AddSingleton<IRabbitMessageConverter, RabbitMessageConverter>();
			services.AddSingleton<IAmqpClient, RabbitAmqpClient>();

			return services;
		}



		/// <summary>
		/// Запуск клиента Rabbit
		/// </summary>
		/// <param name="app"></param>
		public static void UseRabbitClient(this IApplicationBuilder app)
		{
			var amqpServer = app.ApplicationServices.GetRequiredService<IVirtualAmqpClient>();
			amqpServer.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}
