using System;

namespace Notification.Amqp.Server.Abstractions
{
	/// <summary>
	/// 
	/// </summary>
	public class AmqpNotSupportedException : Exception
	{
		public AmqpNotSupportedException()
		{ }

		public AmqpNotSupportedException(string message) : base(message) { }
		public AmqpNotSupportedException(string message, Exception exception) : base(message, exception) { }
	}
}
