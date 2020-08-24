using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Queue.Nats.Core
{
	/// <summary>
	/// Default connection to nats
	/// </summary>
	public class DefaultNatsQueueConnection : INatsQueueConnection
	{
		private readonly ILogger<DefaultNatsQueueConnection> _log;
		private ConcurrentDictionary<string, IConnection> _connections = new ConcurrentDictionary<string, IConnection>();
		private readonly ConnectionFactory _factory = new ConnectionFactory();

		public DefaultNatsQueueConnection(ILogger<DefaultNatsQueueConnection> log)
		{
			_log = log;
		}

		public IConnection CreateConnection(Options options)
		{
			if (string.IsNullOrWhiteSpace(options.Name))
				throw new ArgumentNullException(nameof(options.Name), "Nats options does't must be null or empt");

			var connection = _connections.GetOrAdd(options.Name, s =>
			{
				_log.LogInformation("Create connection");
				return _factory.CreateConnection(options);
			});
			return connection;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var connection in _connections)
			{
				connection.Value?.Dispose();
			}
			_connections.Clear();
		}
	}
}