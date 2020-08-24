using System;
using System.Collections.Generic;
using Queue.Rabbit.Core.Options;

namespace Queue.Rabbit.Server
{
	public interface IRabbitDelayConfig
	{
		void Init(Uri uri, DelayOptions options);
		RabbitDelayInfo GetDelay(TimeSpan delay);
		IEnumerable<RabbitDelayInfo> Queues { get; }
	}
}