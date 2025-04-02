using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client
{
	public static class BasicPropertiesExtensions
	{
		/// <summary>
		/// Заполнить свойства из HttpRequestMessage
		/// </summary>
		/// <param name="props"></param>
		/// <param name="request"></param>
		/// <param name="baseUri"></param>
		/// <returns></returns>
		public static BasicProperties Prepare(this BasicProperties props, HttpRequestMessage request)
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
					headers.TryAddToLowerCase(header.Key, string.Join(";", header.Value));
				}
			}

			headers.TryAddToLowerCase(QueueHeaders.Uri, request.RequestUri.AbsolutePath);
			headers.TryAddToLowerCase(QueueHeaders.Method, request.Method.Method);
			headers.TryAddToLowerCase(HeaderNames.Host, request.RequestUri.Host);

			props.Headers = headers;

			props.DeliveryMode = DeliveryModes.Persistent;
			if (!request.Headers.TryGetValues(QueueHeaders.Priority, out var values) || !byte.TryParse(values.FirstOrDefault(), out var priority))
				priority = 5;

			if (priority > 9)
				priority = 9;

			props.Priority = priority;
			props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

			return props;
		}
	}
}
