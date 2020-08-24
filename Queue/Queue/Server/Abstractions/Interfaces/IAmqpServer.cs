using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Notification.Amqp.Server.Abstractions.Interfaces
{
	/// <summary>
	/// Сервер обработки запросов по Amqp протоколу
	/// </summary>
	public interface IAmqpServer
	{
		/// <summary>
		/// Запуск сервера
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task StartAsync(RequestDelegate requestDelegate, CancellationToken cancellationToken);
		/// <summary>
		/// Остановить сервер
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task StopAsync(CancellationToken cancellationToken);
	}
}