using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Queue.Core.Intefaces
{
	/// <summary>
	/// Клиент для  отправки сообщений к сесурсу
	/// </summary>
	public interface IQueueClient : IDisposable
	{
		/// <summary>
		/// Отправить сообщение к ресурсу
		/// </summary>
		/// <param name="baseUri">Идентификатор ресурса</param>
		/// <param name="request">Запрос к ресурсу</param>
		/// <param name="token"></param>
		/// <returns></returns>
		Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken token);
	}
}