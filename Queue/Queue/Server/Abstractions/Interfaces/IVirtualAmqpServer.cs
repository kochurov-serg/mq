using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Notification.Amqp.Server.Abstractions.Interfaces
{
	/// <summary>
	/// Виртуальный amqp server
	/// </summary>
	public interface IVirtualAmqpServer
	{
		/// <summary>
		/// Запуск сервера
		/// </summary>
		/// <param name="applicationBuilder">Configuring pipeline</param>
		/// <param name="cancellationToken">Токен отмены запуска</param>
		/// <returns></returns>
		Task StartAsync(IApplicationBuilder applicationBuilder, CancellationToken cancellationToken);
		/// <summary>
		/// Остановка сервера
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task StopAsync(CancellationToken cancellationToken);
	}
}