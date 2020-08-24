
namespace Queue.Core
{
	/// <summary>
	/// Заголовки
	/// </summary>
	public static class QueueHeaders
	{
		/// <summary>
		/// Относительный адрес запроса
		/// </summary>
		public const string Uri = "uri";
		/// <summary>
		/// Method
		/// </summary>
		public const string Method = "method";
		/// <summary>
		/// Идентификатор запроса и ответа
		/// </summary>
		public const string CorrelationId = "correlation-Id";
		/// <summary>
		/// Priority
		/// </summary>
		public const string Priority = "priority";
		/// <summary>
		/// Status
		/// </summary>
		public const string Status = "status";
		public const string ReplyTo = "reply-to";
	}
}