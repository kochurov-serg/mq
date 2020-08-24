using System;
using Microsoft.Extensions.Logging;
using Queue.Rabbit.Core.Interfaces;
using RabbitMQ.Client;

namespace Queue.Rabbit.Core
{
	public class RabbitmqConnection: IRabbitmqConnection
	{
		private readonly ILogger<RabbitmqConnection> _logger;
		private readonly ConnectionFactory _factory;
		private IConnection _connection;
		private readonly object _oLock = new object();
		/// <inheritdoc />
		public RabbitmqConnection(ILogger<RabbitmqConnection> logger, RabbitConnection connection)
		{
			_logger = logger;
			_factory = connection.ConnectionFactory;
		}
		/// <inheritdoc />
		public IModel CreateModel()
		{
			if (!ConnectionIsOpen())
			{
				lock (_oLock)
				{
					if (!ConnectionIsOpen())
					{
						_logger.LogInformation($"Create rabbitmq connection host: {_factory.HostName}, user: {_factory.UserName}");
						_connection = _factory.CreateConnection();
						_logger.LogInformation($"connection created host: {_factory.HostName}, user: {_factory.UserName}");
						_connection.ConnectionBlocked += (_, args) =>
						{
							_logger.LogWarning($"Connection blocked {args.Reason}");
						};

						_connection.ConnectionUnblocked += (_, a) =>
						{
							_logger.LogWarning($"Connection unblocked");
						};
					}
				}
			}
			
			return _connection.CreateModel();
		}

		private bool ConnectionIsOpen() => _connection != null && _connection.IsOpen;

		/// <inheritdoc />
		public void Dispose()
		{

			_connection?.Dispose();
		}
	}
}
