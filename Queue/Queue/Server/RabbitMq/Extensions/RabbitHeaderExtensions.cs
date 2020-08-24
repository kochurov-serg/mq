using System.Collections.Generic;
using System.Text;

namespace Notification.Amqp.Server.RabbitMq
{
	/// <summary>
	/// Методы работы над заголовками rabbitmq
	/// </summary>
	internal static class RabbitHeaderExtensions
	{
		/// <summary>
		/// Получить строковое представление заголовка
		/// </summary>
		/// <param name="headers">Заголовки</param>
		/// <param name="key">Ключ для получения значения</param>
		/// <param name="defaultValue">Значение по умолчанию если заголовок отсутствует</param>
		/// <returns></returns>
		public static string GetOrDefaultString(this IDictionary<string, object> headers, string key, string defaultValue = null)
		{
			var value = GetOrDefault(headers, key);
			var bytes = value as byte[];
			if (value == null || bytes == null)
				return defaultValue;

			return Encoding.UTF8.GetString((byte[])value);
		}

		/// <summary>
		/// Извлечение заголовка и удаление эго из списка заголовков
		/// </summary>
		/// <param name="headers">Список заголовков</param>
		/// <param name="key">Ключ</param>
		/// <param name="defaultValue">Значение по умолчанию</param>
		/// <returns></returns>
		public static string ExtractHeader(this IDictionary<string, object> headers, string key,
			string defaultValue = null)
		{
			var value = GetOrDefaultString(headers, key, defaultValue);
			headers.Remove(key);

			return value;
		}

		/// <summary>
		/// Получить заголовок или null
		/// </summary>
		/// <param name="headers">Словарь заголовков</param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static object GetOrDefault(this IDictionary<string, object> headers, string key) =>
			headers.ContainsKey(key) ? headers[key] : null;
	}
}