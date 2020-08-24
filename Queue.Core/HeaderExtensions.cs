using System.Collections.Generic;
using System.Text;

namespace Queue.Core
{
	/// <summary>
	/// Методы работы над заголовками rabbitmq
	/// </summary>
	public static class HeaderExtensions
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

			if (value == null || !(value is byte[] bytes))
				return defaultValue;

			return Encoding.UTF8.GetString(bytes);
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

		public static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, bool replace = true)
		{
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, value);
			}
			else if (replace)
			{
				dictionary[key] = value;
			}
		}

		public static void TryAddToLowerCase<TValue>(this IDictionary<string, TValue> dictionary, string key, TValue value, bool replace = true)
		{
			var lowerKey = key.ToLowerInvariant();
			if (!dictionary.ContainsKey(lowerKey))
			{
				dictionary.Add(lowerKey, value);
			}
			else if (replace)
			{
				dictionary[lowerKey] = value;
			}
		}
	}
}