using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Notification.Amqp.Client.Abstractions.Interfaces
{
	/// <summary>
	/// Виртуальный Клиент отправки сообщений в очередь
	/// </summary>
	public interface IVirtualAmqpClient
	{
		/// <summary>
		/// Отправить сообщение
		/// </summary>
		/// <param name="baseUrl">exchange name</param>
		/// <param name="request">request</param>
		/// <returns></returns>
		Task<HttpResponseMessage> Send(Uri baseUrl, HttpRequestMessage request);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseUri"></param>
		/// <param name="request">http request</param>
		/// <param name="options">options request</param>
		/// <returns></returns>
		Task<HttpResponseMessage> Send(Uri baseUri, HttpRequestMessage request,
			AmqpRequestOptions options);
		/// <summary>
		/// Get or default null value if value not exist
		/// </summary>
		/// <param name="correlationId">Identifier request and response</param>
		/// <returns></returns>
		AmqpResponse Extract(string correlationId);
		/// <summary>
		/// Start client session
		/// </summary>
		/// <returns></returns>
		Task StartAsync();
		Task StopAsync();
	}
}