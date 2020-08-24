using System;
using System.Threading;

namespace Notification.Amqp.Client.Abstractions
{
	/// <summary>
	/// Настройки вызова
	/// </summary>
	public class AmqpRequestOptions
	{
		public AmqpCallType CallType { get; set; }
		/// <summary>
		/// Время Ожидания ответа (CallType)
		/// </summary>
		public int Timeout { get; set; }

		public TimeSpan Expires
		{
			get => Timeout == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(Timeout);
		}

		/// <summary>
		/// Токен отмены отправки запроса
		/// </summary>
		public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

		/// <summary>
		/// Настройки по умолчанию
		/// </summary>
		public static AmqpRequestOptions DefaultOptions => new AmqpRequestOptions
		{
			CancellationToken = CancellationToken.None,
			Timeout = 0,
			CallType = AmqpCallType.Call
		};
	}

	/// <summary>
	/// Тип вызова сервиса
	/// </summary>
	public enum AmqpCallType
	{
		/// <summary>
		/// Вызвал и забыл
		/// </summary>
		Call,
		/// <summary>
		/// Ожидание ответа после отправки запроса
		/// </summary>
		Rpc,
		/// <summary>
		/// Авинхронный вызов, ответ будет обработан позже
		/// </summary>
		Async
	}
}
