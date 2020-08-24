using System;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Options;

namespace Queue.Rabbit.Server
{
	public class AppQueueOption : QueueOption
	{
		public AppQueueOption()
		{
			Uri = new UriBuilder(RabbitConsts.Schema, "localhost", 80).Uri;
		}

		public bool AutoAck { get; set; }

		/// <inheritdoc />
		public override string ToString() => $"queue {Uri}";

	}
}