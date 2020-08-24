using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Rabbit.Server.Interfaces;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server
{
	/// <summary>
	/// Сервер обработки запросов от Rabbit
	/// </summary>
	public class RabbitQueueServer : IRabbitQueueServer
	{
		private bool _isDisposable;
		private readonly ILogger<RabbitQueueServer> _log;
		private readonly IQueueConverter<BasicDeliverEventArgs> _converter;
		private readonly IRabbitCommunicationServer _communication;
		private readonly IRabbitResponseProcessed _responseProcessed;


		/// <inheritdoc />
		public RabbitQueueServer(ILogger<RabbitQueueServer> log, IQueueConverter<BasicDeliverEventArgs> converter, IRabbitCommunicationServer communication, IRabbitResponseProcessed responseProcessed)
		{
			_log = log;
			_converter = converter;
			_communication = communication;
			_responseProcessed = responseProcessed;
		}

		/// <inheritdoc />
		public async Task Start(RequestDelegate requestDelegate, CancellationToken cancellationToken)
		{
			await _communication.Init();

			_communication.CreateBasicConsumer(async (_, args) =>
			{
				try
				{
					_log.LogTrace($"Incoming request");

					if (cancellationToken.IsCancellationRequested)
					{
						_log.LogTrace("Cancellation token call. {queueName}", args.RoutingKey);
						_communication.Dispose();
						return;
					}

					_log.LogTrace("Parsing request");
					var features = new FeatureCollection();
					features.Set(new ItemsFeature());
					var context = await _converter.Parse(args, features);

					if (context != null)
					{
						await requestDelegate(context);
						await _responseProcessed.Handle(args, context);
					}
					else
					{
						await _communication.Ack(args);
						await _communication.SendError(new BasicDeliverEventArgs
						{
							BasicProperties = args.BasicProperties,
							Exchange = args.Exchange,
							Body = args.Body
						});
					}
				}
				catch (Exception e)
				{
					_log.LogError(e, "Error processing message");
					args.BasicProperties.Headers.TryAddToLowerCase("exception", e.ToString());
					await _communication.SendException(args);
				}
			});
		}

		/// <inheritdoc />
		public Task Stop(CancellationToken cancellationToken)
		{
			Dispose();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			if (_isDisposable)
				return;

			_communication.Dispose();
			_isDisposable = true;
		}
	}
}