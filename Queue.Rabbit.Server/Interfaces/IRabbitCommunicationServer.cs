using System;
using System.Threading;
using System.Threading.Tasks;
using Queue.Rabbit.Core.Repeat;
using Queue.Server.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server.Interfaces;

public interface IRabbitCommunicationServer : IDisposable
{
    Task Init();

    /// <summary>
    /// Создать подключение
    /// </summary>
    /// <param name="received">Подписчик</param>
    Task CreateBasicConsumer(AsyncEventHandler<BasicDeliverEventArgs> received);

    /// <summary>
    /// Отправить данные
    /// </summary>
    /// <returns></returns>
    Task Send(string exchange, string routingKey, BasicProperties basicProperties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken);
    
    /// <summary>
    /// Ack message
    /// </summary>
    /// <param name="deliveryTag"></param>
    /// <returns></returns>
    Task Ack(ulong deliveryTag);

    Task TryAck(ulong deliveryTag);

    /// <summary>
    /// Wait and retry message
    /// </summary>
    /// <param name="args">message</param>
    /// <param name="config">retry queue</param>
    /// <returns></returns>
    Task<bool> Retry(string exchange, BasicProperties basicProperties, ReadOnlyMemory<byte> body, RepeatConfig config, ulong deliveryTag, CancellationToken cancellationToken);

    Task<bool> Retry(BasicDeliverEventArgs args, QueueContext context);
}