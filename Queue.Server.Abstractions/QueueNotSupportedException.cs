using System;

namespace Queue.Server.Abstractions
{
	/// <summary>
	/// 
	/// </summary>
	public class QueueNotSupportedException : Exception
	{
		public QueueNotSupportedException()
		{ }

		public QueueNotSupportedException(string message) : base(message) { }
		public QueueNotSupportedException(string message, Exception exception) : base(message, exception) { }
	}
}
