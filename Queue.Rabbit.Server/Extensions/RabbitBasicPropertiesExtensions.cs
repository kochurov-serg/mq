using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Repeat;
using RabbitMQ.Client;

namespace Queue.Rabbit.Server.Extensions
{

	public static class RabbitBasicPropertiesExtensions
	{
		public static IBasicProperties AddRetry(this IBasicProperties properties, RepeatConfig config, RabbitDelayInfo delay)
		{
			var headers = properties.Headers;
			var ttl = config.Strategy == RepeatStrategy.Const
				? delay.Option.Ttl
				: delay.Next?.Option?.Ttl ?? delay.Option.Ttl;
			headers.TryAddToLowerCase(RepeatConfig.StrategyRepeatDelay, config.Strategy.ToString().ToLower(), true);
			headers.TryAddToLowerCase(RepeatConfig.RepeatDelay, ttl.ToString(), true);
			headers.TryAddToLowerCase(RepeatConfig.RepeatCount, (config.Count - 1).ToString(CultureInfo.InvariantCulture), true);

			return properties;
		}

		public static IBasicProperties CreateBasicPropertiesResponse(this IBasicProperties props, IBasicProperties requestProps, HttpRequest request, HttpResponse response)
		{
			var headers = response.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.ToString() as object);
			headers.TryAddToLowerCase(HeaderNames.ContentType, response.ContentType);
			headers.TryAddToLowerCase(HeaderNames.ContentLength, response.Body.Length.ToString());
			headers.TryAddToLowerCase(QueueHeaders.Status, response.StatusCode.ToString(CultureInfo.InvariantCulture));
			headers.TryAddToLowerCase(QueueHeaders.Uri, request.Path.Value);
			
			props.Headers = headers;

			props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

			if (!string.IsNullOrWhiteSpace(requestProps.CorrelationId))
			{
				props.CorrelationId = requestProps.CorrelationId;
			}

			props.DeliveryMode = 2;
			props.Priority = requestProps.Priority;

			if (!string.IsNullOrWhiteSpace(requestProps.MessageId))
			{
				props.MessageId = requestProps.MessageId;
			}

			if (!string.IsNullOrWhiteSpace(requestProps.UserId))
			{
				props.UserId = requestProps.UserId;
			}

			return props;
		}
	}
}
