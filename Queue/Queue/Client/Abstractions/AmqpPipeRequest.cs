using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.Amqp.Client.Abstractions
{
	/// <summary>
	/// Pipe call type. Next call by success response and send response or request
	/// </summary>
	public enum AmqpPipeRequest
	{
		/// <summary>
		/// Request next send pipe
		/// </summary>
		Request,
		/// <summary>
		/// Response next send pipe
		/// </summary>
		Response,
	}
}
