using System;

namespace Notification.Amqp.Client.Abstractions
{
	/// <summary>
	/// Время обработки запроса
	/// </summary>
	public class TimeProcessing
	{
		/// <summary>
		/// Время начала обработки запроса
		/// </summary>
		public long StartTime { get; set; }
		/// <summary>
		/// Время окончания обработки запроса
		/// </summary>
		public long EndTime { get; set; }
		/// <summary>
		/// Время отправки запроса в миллисекундах
		/// </summary>
		public long StartSendRequestTime { get; set; }
		/// <summary>
		/// Время затраченное на обработку запроса
		/// </summary>
		public TimeSpan Elapsed => TimeSpan.FromMilliseconds(EndTime - StartTime);
		/// <summary>
		/// Время затраченное на отправку сообщения
		/// </summary>
		public TimeSpan ElapsedRequest => TimeSpan.FromMilliseconds(EndTime - StartSendRequestTime);
	}
}