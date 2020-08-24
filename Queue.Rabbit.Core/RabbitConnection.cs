using RabbitMQ.Client;

namespace Queue.Rabbit.Core
{
	/// <summary>
	/// Настройки подключения
	/// </summary>
	public class RabbitConnection
	{
		/// <summary>
		/// Настройки соединения
		/// </summary>
		public ConnectionFactory ConnectionFactory { get; set; }
	}
}