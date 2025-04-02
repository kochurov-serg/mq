using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client;

public class DefaultRabbitConnectionFactory : IRabbitConnectionFactory
{
    private readonly ConcurrentDictionary<string, IConnection> _connections = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public static string CreateKey(ConnectionFactory factory) => string.Concat(factory.HostName, "@", factory.HostName, factory.Port, factory.VirtualHost);

    public async Task<IConnection> CreateConnection(ConnectionFactory factory)
    {
        var key = CreateKey(factory);
        if (_connections.TryGetValue(key, out var connection))
        {
            return connection;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_connections.TryGetValue(key, out connection))
            {
                return connection;
            }

            connection = await factory.CreateConnectionAsync();
            _connections.TryAdd(key, connection);
            return connection;
        }
        finally
        {
            _semaphore.Release();
        }
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