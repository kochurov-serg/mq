namespace Notification.Amqp.Server.RabbitMq
{
	/// <summary>
	/// Ключевые параметры взаимодействия клиента и сервера
	/// </summary>
	public class AmqpCommunication
	{
		/// <summary>
		/// Время на получение ответа
		/// </summary>
		public const int RPC_TIMEOUT = 90;
		public const int Call_TIMEOUT = 90;

		public const string ProtocolName = "amqp";
		/// <summary>
		/// Тип данных для обмена по умолчанию
		/// </summary>
		public const string DefaultContentType = "application/json";
		/// <summary>
		/// Фрагмент очереди с ошибками обработки сообщений
		/// </summary>
		private const string _error = "error";
		/// <summary>
		/// Фрагмент очереди ресурса к которому производилось обращение
		/// </summary>
		private static string Resource = "resource";
		/// <summary>
		/// Фрагмент очереди запросов к ресурсу
		/// </summary>
		private const string _request = "request";
		/// <summary>
		/// Фрагмент очереди ответов от ресурса
		/// </summary>
		private const string _response = "response";
		/// <summary>
		/// Фрагмент очереди клиента
		/// </summary>
		public static string Client = "client";
		/// <summary>
		/// Фрагмент очереди с отложенной обработкой
		/// </summary>
		private const string _delay = "delay";
		/// <summary>
		/// Разделитель блоков очередей
		/// </summary>
		public static string Delimiter = "/";

		/// <summary>
		/// Наименование очереди запросов к сервису
		/// </summary>
		public static string ResourceRequestQueue
		{
			get => Resource + Delimiter + _request;
		}

		/// <summary>
		/// Наименование очереди ошибок обработки сервиса
		/// </summary>
		public static string ResourceErrorQueue
		{
			get => Resource + Delimiter + _error;
		}

		public static string ResourceDelayQueue
		{
			get => Resource + Delimiter + _delay;
		}

		/// <summary>
		/// Наименование очереди обработки ошибок клиента
		/// </summary>
		public static string ClientErrorQueue
		{
			get => Client + Delimiter + _error;
		}
		/// <summary>
		/// Очередь отложенных сообщений
		/// </summary>
		public static string ClientDelayQueue { get => Client + Delimiter + _delay; }
		/// <summary>
		/// Наименование очереди ответов от ресурса
		/// </summary>
		public static string ClientResponseQueue { get => Client + Delimiter + _response; }


		public static string Join(params string[] values) => string.Join(Delimiter, values);
	}
}
