using System;
using RabbitMQ.Client;

namespace Notification.Amqp.Server.RabbitMq
{
	public class AmqpConnection
	{
		/// <summary>
		/// Настройки соединения
		/// </summary>
		public ConnectionFactory ConnectionFactory { get; set; }
	}

	public class AmqpServerOptions
	{
		/// <summary>
		/// Наименование ресурса 
		/// </summary>
		public Uri ServerName { get; set; }

		/// <summary>
		/// Количество попыток
		/// </summary>
		public int CountRetry { get; set; }

		public AmqpConnection Connection { get; set; }
	}

	public class AmqpClientOptions
	{
		/// <summary>
		/// Client name. 
		/// </summary>
		/// <remarks>Ferm application need guaranteed unique client. If client name be not unique</remarks>
		public string ClientName { get; set; }
		/// <summary>
		/// Количество попыток
		/// </summary>
		public int CountRetry { get; set; }

		public AmqpConnection Connection { get; set; }
	}
}