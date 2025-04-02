using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client;

/// <summary>
/// Connection factory store by user name, host, port, virtual host
/// </summary>
public interface IRabbitConnectionFactory : IDisposable
{
    Task<IConnection> CreateConnection(ConnectionFactory factory);
}