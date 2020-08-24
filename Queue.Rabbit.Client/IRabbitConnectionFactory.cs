using System;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client
{
	/// <summary>
	/// Connection factory store by user name, host, port, virtual host
	/// </summary>
	public interface IRabbitConnectionFactory : IDisposable
	{
		IConnection CreateConnection(ConnectionFactory factory);
	}
}