using System;
using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client
{
	public class DefaultRabbitConnectionFactory : IRabbitConnectionFactory
	{
		private readonly ConcurrentDictionary<string, IConnection> _connections = new ConcurrentDictionary<string, IConnection>();

		public static string CreateKey(ConnectionFactory factory) => string.Concat(factory.HostName, "@", factory.HostName, factory.Port, factory.VirtualHost);

		public IConnection CreateConnection(ConnectionFactory factory)
		{
			var key = CreateKey(factory);
			var connection = _connections.GetOrAdd(key, key =>
			{
				return factory.CreateConnection();
			});

			return connection;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var connection in _connections)
			{
				connection.Value.Dispose();
			}

			_connections.Clear();
		}
	}
}
