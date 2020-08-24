using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Queue.Server.Abstractions.Interfaces
{
	/// <summary>
	/// Сервер обработки запросов по Mqp протоколу
	/// </summary>
	public interface IQueueServer
	{
		/// <summary>
		/// Запуск сервера
		/// </summary>
		/// <param name="requestDelegate">Request delegating handler</param>
		/// <param name="cancellationToken">cancellation token</param>
		/// <returns></returns>
		Task Start(RequestDelegate requestDelegate, CancellationToken cancellationToken);
		/// <summary>
		/// Остановить сервер
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task Stop(CancellationToken cancellationToken);
	}
}