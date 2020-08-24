using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Queue.Kafka.Client;
using Queue.Kafka.Client.HttpClientBuilder;
using Queue.Kafka.Server;
using Queue.Nats.Client;
using Queue.Nats.Server;
using Queue.Rabbit.Client;
using Queue.Rabbit.Client.HttpClientBuilder;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Options;
using Queue.Rabbit.Server;
using Queue.Rabbit.Server.Extensions;
using RabbitMQ.Client;

namespace Test
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var connection = new RabbitConnection
			{
				ConnectionFactory = new ConnectionFactory
				{
					HostName = "127.0.0.1",
					Port = 5672,
					UserName = "guest",
					Password = "guest"
				}
			};


			var clientOption = new RabbitClientOptions
			{
				ClientName = Guid.NewGuid().ToString("N"),
				ClientUnique = Guid.NewGuid().ToString("N").Substring(0, 6),
				Connection = connection
			};
			services.AddRabbitClient(provider => clientOption);
			//services.AddHttpClient<PersonalClient>().ConfigurePrimaryRabbitMessageHandler(connection.ConnectionFactory, provider => clientOption);

			//services.AddHttpClient<PersonalClient>().ConfigurePrimaryHttpMessageHandler(provider => provider.GetService<NatsMessageHandler>());

			//services.AddHttpClient<PersonalClient>().ConfigurePrimaryKafkaMessageHandler(provider => new KafkaOptions
			//{
			//	ProducerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" },
			//	ConsumerConfig = new ConsumerConfig
			//	{
			//		ClientId = "test",
			//		GroupId = "test-consumer-group",
			//		BootstrapServers = "localhost:9092",
			//		AutoOffsetReset = AutoOffsetReset.Earliest
			//	}
			//});
			//var options = NATS.Client.ConnectionFactory.GetDefaultOptions();
			//options.Name = "localhost";
			//options.Servers = new[] { "nats://localhost:4222" };

			//services.AddNatsClient(provider => new NatsQueueClientOption
			//{
			//	Options = options
			//});

			//	adminBuilder.
			//	.AddNatsServer(provider => new NatsServerOption
			//{
			//	Options = options,
			//	Server = new Uri("nats://localhost")
			//});
			//services.AddKafkaServer(provider => new KafkaServerOption
			//{
			//	Server = new List<Uri> {new Uri("kafka://localhost")},
			//	ProducerConfig = new ProducerConfig {BootstrapServers = "localhost:9092"},
			//	ConsumerConfig = new ConsumerConfig
			//	{
			//		ClientId = "personal-area",
			//		GroupId = "test-consumer-group",
			//		BootstrapServers = "localhost:9092",
			//		AutoOffsetReset = AutoOffsetReset.Earliest
			//	}
			//});
			services.AddRabbitServer(new RabbitServerOptions
			{
				Connection = connection,
				DelayOptions = new DelayOptions
				{
					QueueOptions = DelayOptions.CreateInterval().Take(3).ToList()
				},
				Queue = new AppQueueOption
				{
					Uri = new Uri("rabbitmq://localhost")
				}
			});
			services.Configure<RabbitMessageHandler>(handler =>
			{
				handler.Factory = null;
			});
			services.AddControllers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			//app.UseHttpsRedirection();
			app.UseRouting();
			app.UseEndpoints(builder => builder.MapControllers());
			var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger("application");
			//app.UseNatsServer(CancellationToken.None).ContinueWith(task =>
			//{
			//	if (task.IsCanceled || task.IsFaulted)
			//	{
			//		if (task.Exception != null)
			//			logger.LogError(task.Exception, "Ошибка привязки Rabbit server");
			//	}
			//});
			app.UseRabbitServer(CancellationToken.None).ContinueWith(task =>
			{
				if (task.IsCanceled || task.IsFaulted)
				{
					if (task.Exception != null)
						logger.LogError(task.Exception, "Ошибка привязки Rabbit server");
				}
			});

			//app.UseKafkaServer(CancellationToken.None).ContinueWith(task =>
			//{
			//	if (task.IsCanceled || task.IsFaulted)
			//	{
			//		if (task.Exception != null)
			//			logger.LogError(task.Exception, "Ошибка привязки kafka server");
			//	}
			//});
		}
	}
}
