namespace Notification.Amqp.Server.Abstractions
{
	/// <summary>
	/// Заголовоки Amqp протокола
	/// </summary>
	public static class AmqpHeaders
	{
		/// <summary>
		/// Метод обращения к сервису
		/// </summary>
		public static string Method = "Method";
		/// <summary>
		/// Статус
		/// </summary>
		public static string StatusCode = "Status-Code";
		/// <summary>
		/// Относительный адрес запроса
		/// </summary>
		public static string Uri = "Uri";
		/// <summary>
		/// Идентификатор запроса и ответа
		/// </summary>
		public static string CorrelationId = "Correlation-Id";
		/// <summary>
		/// Количество попыток обработки запроса
		/// </summary>
		public static string RetryCount = "Retry";
	}
}