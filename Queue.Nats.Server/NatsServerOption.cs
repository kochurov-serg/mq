using System;

namespace Queue.Nats.Server
{
	public class NatsServerOption
	{
		public Uri Server { get; set; }
		public NATS.Client.Options Options { get; set; }

		public bool ForceResponseStream { get; set; }
	}
}