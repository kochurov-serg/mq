using NATS.Client;

namespace Queue.Nats.Client
{
	/// <summary>
	/// Nats client option
	/// </summary>
	public class NatsQueueClientOption
	{
		public const string ConnectionProperty = "natsConnection";
		public Options Options { get; set; }
	}
}