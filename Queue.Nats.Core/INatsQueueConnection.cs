using System;
using NATS.Client;

namespace Queue.Nats.Core
{
	/// <summary>
	/// Connection to Nats
	/// </summary>
	public interface INatsQueueConnection : IDisposable
	{
		IConnection CreateConnection(Options options);
	}
}