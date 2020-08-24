using System;
using System.Globalization;
using System.Net.Http;
using Queue.Rabbit.Core.Options;

namespace Queue.Rabbit.Core.Repeat
{
	public static class HttpRequestExtensions
	{
		public static HttpRequestMessage AddRetry(this HttpRequestMessage message, RepeatConfig config)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (config == null)
				throw new ArgumentNullException(nameof(config));

			if (message.Headers == null)
				throw new ArgumentNullException(nameof(message.Headers));


			message.Headers.Add(RepeatConfig.RepeatCount, config.Count.ToString(CultureInfo.InvariantCulture));
			message.Headers.Add(RepeatConfig.RepeatDelay, config.Delay.ToString());
			message.Headers.Add(RepeatConfig.StrategyRepeatDelay, config.Strategy.ToString().ToLower());

			return message;
		}
	}
}