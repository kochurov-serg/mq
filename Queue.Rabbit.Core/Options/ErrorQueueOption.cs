using System;

namespace Queue.Rabbit.Core.Options
{
	public class ErrorQueueOption : QueueOption
	{
		public ErrorQueueOption()
		{
			Uri = new Uri("error", UriKind.Relative);
		}

		public override string ToString() => $"queue {Uri}";
	}
}