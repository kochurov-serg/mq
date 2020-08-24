using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Notification.Amqp.Client.Abstractions.Interfaces
{
	/// <summary>
	/// Клиент для  отправки сообщений к сесурсу
	/// </summary>
	public interface IAmqpClient : IDisposable
	{
		/// <summary>
		/// Отправить сообщение к ресурсу
		/// </summary>
		/// <param name="baseUri">Идентификатор ресурса</param>
		/// <param name="request">Запрос к ресурсу</param>
		/// <param name="options">Настройки параметров запроса</param>
		/// <returns></returns>
		Task Send(Uri baseUri, HttpRequestMessage request, AmqpRequestOptions options);
		/// <summary>
		/// Подписка на события ответа от ресурса
		/// </summary>
		/// <returns></returns>
		Task Subscribe(Func<HttpResponseMessage, bool> callback);
	}
}