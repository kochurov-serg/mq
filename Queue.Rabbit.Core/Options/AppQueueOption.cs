using System;

namespace Queue.Rabbit.Core.Options
{
	public class MainQueueOption : QueueOption
	{
		public MainQueueOption()
		{
			Uri = new UriBuilder(RabbitConsts.Schema, "localhost", 80).Uri;
		}

		/// <inheritdoc />
		public override string ToString() => $"queue {Uri}";
	}
}