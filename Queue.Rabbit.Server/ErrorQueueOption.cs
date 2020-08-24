using System;
using Queue.Rabbit.Core.Options;

namespace Queue.Rabbit.Server
{
	public class ErrorQueueOption : QueueOption
	{
		public ErrorQueueOption()
		{
			Uri = new Uri("error", UriKind.Relative);
			ExpiresQueue = TimeSpan.Zero;
			Ttl =TimeSpan.FromDays(60);
		}

		public override string ToString() => $"queue {Uri}";
	}
}