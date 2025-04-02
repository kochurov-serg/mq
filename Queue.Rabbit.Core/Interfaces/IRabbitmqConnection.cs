using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Queue.Rabbit.Core.Interfaces;

/// <summary>
/// Создание каналов
/// </summary>
public interface IRabbitmqConnection: IDisposable
{
    /// <summary>
    /// Создание модели
    /// </summary>
    /// <returns></returns>
    Task<IChannel> CreateModel();
}