using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Notification.Amqp.Client.Abstractions
{
	/// <summary>
	/// Ответ от Amqp сервера
	/// </summary>
	public class AmqpResponse
	{
		public TaskStatus Status { get; set; } = TaskStatus.Created;
		/// <summary>
		/// Время затраченное на обработку запроса
		/// </summary>
		public TimeProcessing TimeProcessing { get; } = new TimeProcessing
		{
			StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
		};

		public void EndTiming()
		{
			TimeProcessing.EndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		public void EndTiming(TaskStatus status)
		{
			EndTiming();
			Status = status;
		}

		/// <summary>
		/// Настройки отправки сообщения
		/// </summary>
		public AmqpRequestOptions RequestOptions { get; set; }
		/// <summary>
		/// Ответ
		/// </summary>
		public TaskCompletionSource<HttpResponseMessage> Response { get; set; } = new TaskCompletionSource<HttpResponseMessage>();

		public Exception Exception { get; set; }
	}
}