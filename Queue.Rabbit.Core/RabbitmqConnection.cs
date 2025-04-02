using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Queue.Rabbit.Core.Interfaces;
using RabbitMQ.Client;

namespace Queue.Rabbit.Core;

public class RabbitmqConnection : IRabbitmqConnection
{
    private readonly ILogger<RabbitmqConnection> _logger;
    private readonly ConnectionFactory _factory;
    private IConnection _connection;
    private readonly Semaphore _semaphore = new(1,1);
    
    /// <inheritdoc />
    public RabbitmqConnection(ILogger<RabbitmqConnection> logger, RabbitConnection connection)
    {
        _logger = logger;
        _factory = connection.ConnectionFactory;
    }
    /// <inheritdoc />
    public async Task<IChannel> CreateModel()
    {
        await CreateConnection().ConfigureAwait(false);

        return await _connection.CreateChannelAsync().ConfigureAwait(false);
    }

    private async Task CreateConnection()
    {
        if (ConnectionIsOpen())
            return;

        _semaphore.WaitOne();

        try
        {
            if (ConnectionIsOpen())
                return;
                
            _logger.LogInformation($"Create rabbitmq connection host: {_factory.HostName}, user: {_factory.UserName}");
            _connection = await _factory.CreateConnectionAsync().ConfigureAwait(false);
            _logger.LogInformation($"connection created host: {_factory.HostName}, user: {_factory.UserName}");
            _connection.ConnectionBlockedAsync += (_, args) =>
            {
                _logger.LogWarning($"Connection blocked {args.Reason}");
                return Task.CompletedTask;
            };

            _connection.ConnectionUnblockedAsync += (_, a) =>
            {
                _logger.LogWarning($"Connection unblocked");
                return Task.CompletedTask;
            };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool ConnectionIsOpen() => _connection is { IsOpen: true };

    /// <inheritdoc />
    public void Dispose()
    {

        _connection?.Dispose();
    }
}