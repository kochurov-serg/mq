using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using Queue.Rabbit.Core.Repeat;
using RabbitMQ.Client;

namespace Queue.Rabbit.Server.Extensions;

public static class RabbitBasicPropertiesExtensions
{
    public static BasicProperties AddRetry(this BasicProperties properties, RepeatConfig config, RabbitDelayInfo delay)
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

    public static BasicProperties CreateBasicPropertiesResponse(this BasicProperties props, HttpRequest request, HttpResponse response)
    {
        var headers = response.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.ToString() as object);
        headers.TryAddToLowerCase(HeaderNames.ContentType, response.ContentType);
        headers.TryAddToLowerCase(HeaderNames.ContentLength, response.Body.Length.ToString());
        headers.TryAddToLowerCase(QueueHeaders.Status, response.StatusCode.ToString(CultureInfo.InvariantCulture));
        headers.TryAddToLowerCase(QueueHeaders.Uri, request.Path.Value);
			
        props.Headers = headers;

        props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        return props;
    }
}