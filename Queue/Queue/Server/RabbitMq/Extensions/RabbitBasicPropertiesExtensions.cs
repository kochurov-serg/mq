using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Notification.Amqp.Server.Abstractions;
using RabbitMQ.Client;

namespace Notification.Amqp.Server.RabbitMq
{

	public static class RabbitBasicPropertiesExtensions
	{
		/// <summary>
		/// Заполнить свойства из HttpRequestMessage
		/// </summary>
		/// <param name="props"></param>
		/// <param name="request"></param>
		/// <param name="baseUri"></param>
		/// <returns></returns>
		public static IBasicProperties Prepare(this IBasicProperties props, HttpRequestMessage request, Uri baseUri)
		{
			if (props == null)
				throw new ArgumentNullException(nameof(props));

			if (request == null)
				throw new ArgumentNullException(nameof(request));

			var headers = request.Headers.ToDictionary(x => x.Key, pair => (object)string.Join(";", pair.Value));

			if (request.Content != null)
			{
				foreach (var header in request.Content.Headers)
				{
					headers.TryAdd(header.Key, string.Join(";", header.Value));
				}
			}

			headers.TryAdd(AmqpHeaders.Uri, request.RequestUri);
			headers.TryAdd(AmqpHeaders.Method, request.Method.Method);
			headers.TryAdd(AmqpHeaders.CorrelationId, request.Method.Method);
			headers.TryAdd(HeaderNames.Host, baseUri.OriginalString);

			props.Headers = headers;

			props.DeliveryMode = 2;
			props.Priority = 5;
			props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

			return props;
		}

		/// <summary>
		/// Увеличения номера попытки
		/// </summary>
		/// <param name="props"></param>
		/// <returns></returns>
		public static int IncrementRetry(this IBasicProperties props)
		{
			var headers = props.Headers;
			var retryCount = headers.GetOrDefaultString(AmqpHeaders.RetryCount);

			if (!int.TryParse(retryCount, out var retry))
			{
				retry = 0;
			}

			retry += 1;
			headers[AmqpHeaders.RetryCount] = retry;
			return retry;
		}

		public static IBasicProperties CreateBasicPropertiesResponse(this IBasicProperties props, IBasicProperties requestProps, HttpResponse response)
		{

			var headers = response.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.ToString() as object);
			headers.TryAdd(HeaderNames.ContentType, response.ContentType);
			headers.TryAdd(HeaderNames.ContentLength, response.Body.Length);
			headers.TryAdd(AmqpHeaders.StatusCode, response.StatusCode.ToString());

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
