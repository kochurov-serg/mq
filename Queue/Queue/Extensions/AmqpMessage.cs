using System.Collections.Generic;
using System.IO;

namespace Notification.Amqp.Extensions
{
	public class AmqpMessage
	{
		/// <summary>
		/// Данные для отправки
		/// </summary>
		public Stream Stream { get; set; }
		/// <summary>
		/// Заголовки
		/// </summary>
		public IDictionary<string, List<string>> Headers { get; set; }
		public IDictionary<string, List<string>> Properties { get; set; }
	}
}
