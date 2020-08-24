using Queue.Rabbit.Core.Options;

namespace Queue.Rabbit.Server
{
	public class RabbitDelayInfo
	{
		public DelayQueueOption Option { get; set; }

		public string QueueName { get; set; }

		public RabbitDelayInfo Next { get; set; }
	}
}