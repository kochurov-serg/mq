using System.Collections.Generic;

namespace Notification.Amqp.Client.Abstractions.Interfaces
{
	/// <summary>
	/// Интерфейс отправки запросов
	/// </summary>
	public interface IHttpRequestPropertiesParser
	{
		/// <summary>
		/// Парсинг настроек отправки запроса из свойств запроса
		/// </summary>
		/// <param name="options"></param>
		/// <param name="properties"></param>
		/// <returns></returns>
		AmqpRequestOptions Parse(AmqpRequestOptions options, IDictionary<string, object> properties);
	}
}