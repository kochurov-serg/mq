using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Notification.Amqp.Client.Abstractions.Interfaces;

namespace Notification.Amqp.Client.Abstractions
{
	/// <inheritdoc />
	public class HttpRequestPropertiesParser : IHttpRequestPropertiesParser
	{
		private readonly ILogger<HttpRequestPropertiesParser> _log;

		/// <inheritdoc />
		public HttpRequestPropertiesParser(ILogger<HttpRequestPropertiesParser> log)
		{
			_log = log;
		}

		public AmqpRequestOptions Parse(AmqpRequestOptions options, IDictionary<string, object> properties)
		{
			if (properties == null)
			{
				return options;
			}

			if (properties.TryGetValue(nameof(options.Timeout).ToLower(), out var timeoutProperties))
			{
				if (int.TryParse(timeoutProperties as string, out var timeout))
				{
					options.Timeout = timeout;
				}
				else
				{
					_log.LogWarning("Timeout Header exists in request properties. but not int value (count seconds)");
				}
			}

			if (properties.TryGetValue(nameof(options.CallType).ToLower(), out var callTypeProperies))
			{
				if (callTypeProperies is AmqpCallType callType ||
					Enum.TryParse(callTypeProperies as string, true, out callType))
				{
					options.CallType = callType;
				}
				else
				{
					_log.LogWarning("Timeout Header exists in request properties. but not int value (count seconds)");
				}
			}

			if (properties.TryGetValue(nameof(options.CancellationToken).ToLower(), out var cancellationTokenProperties))
			{
				if (cancellationTokenProperties is CancellationToken cancellationToken)
				{
					options.CancellationToken = cancellationToken;
				}
				else
				{
					_log.LogWarning("Timeout Header exists in request properties. but not int value (count seconds)");
				}
			}

			return options;
		}
	}
}
