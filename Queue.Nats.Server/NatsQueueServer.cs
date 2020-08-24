using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using NATS.Client;
using Queue.Nats.Core;
using Queue.Nats.Server.Interfaces;

namespace Queue.Nats.Server
{
	public class NatsQueueServer : INatsQueueServer, IDisposable
	{
		private readonly ILogger<NatsQueueServer> _log;
		private readonly INatsQueueConnection _connection;
		private readonly NatsServerOption _options;
		private readonly INatsConverter _converter;
		private readonly INatsResponseProcessed _responseProcessed;
		private IAsyncSubscription _subscription;

		public NatsQueueServer(ILogger<NatsQueueServer> log, INatsQueueConnection connection, NatsServerOption options, INatsConverter converter, INatsResponseProcessed responseProcessed)
		{
			_log = log;
			_connection = connection;
			_options = options;
			_converter = converter;
			_responseProcessed = responseProcessed;
		}

		/// <inheritdoc />
		public Task Start(RequestDelegate requestDelegate, CancellationToken cancellationToken)
		{

			var connection = _connection.CreateConnection(_options.Options);
			_log.LogTrace("NATS connected. Subscribing to queue {host}", _options.Server.Host);

			_subscription = connection.SubscribeAsync(_options.Server.Host, async (_, args) =>
			{
				try
				{
					var context = await _converter.Parse(args, new FeatureCollection());

					if (context != null)
					{
						await requestDelegate(context);
						await _responseProcessed.ProcessResponse(args, context, connection);
					}
					else
					{
						_log.LogError("Cannot parse context");
					}
				}
				catch (Exception e)
				{
					_log.LogError(e, $"Nats server not processed request {args.Message.Subject}");
				}
			});

			_log.LogTrace("NATS server subscribed");
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task Stop(CancellationToken cancellationToken)
		{
			Dispose();
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_subscription?.Unsubscribe();
			_subscription?.Dispose();
			_connection?.Dispose();
		}
	}
}
