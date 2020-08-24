using System;
using System.Collections.Generic;
using System.Linq;
using Queue.Core;
using Queue.Rabbit.Core.Options;

namespace Queue.Rabbit.Server
{
	public class RabbitDelayConfig : IRabbitDelayConfig
	{
		private DelayOptions _options;
		private Dictionary<TimeSpan, RabbitDelayInfo> _delays;
		private RabbitDelayInfo _first;

		public void Init(Uri uri, DelayOptions options)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			_options = options;
			var ordered = options.QueueOptions.GroupBy(x => x.Ttl).OrderBy(x => x.Key).Select(x => x.First()).ToArray();
			_delays = new Dictionary<TimeSpan, RabbitDelayInfo>(ordered.Length);
			RabbitDelayInfo next = null;
			for (int i = ordered.Length - 1; i >= 0; i--)
			{
				var delay = ordered[i];
				var info = new RabbitDelayInfo
				{
					Option = delay,
					Next = next,
					QueueName = QueueNameExtensions.GetQueue(uri, delay.Uri)
				};
				_delays.Add(delay.Ttl, info);
				next = info;
			}

			_first = next;
		}

		public RabbitDelayInfo GetDelay(TimeSpan delay)
		{
			if (_delays == null)
				throw new Exception($"{nameof(RabbitDelayConfig)} not be initialized");

			if (_delays.Count == 0)
				return null;

			if (_delays.ContainsKey(delay))
				return _delays[delay];

			var info = _first;
			for (int i = 0; i < _delays.Count - 1; i++)
			{
				if (delay < info.Next.Option.Ttl)
					return info;

				info = info.Next;
			}

			return info;
		}

		public IEnumerable<RabbitDelayInfo> Queues => _delays.Select(x => x.Value);
	}
}