using System;
using System.Linq;
using System.Net.Http.Headers;

namespace Queue.Core
{
	public static class HttpHeadersExtensions
	{
		/// <summary>
		/// Добавить заголовок идентифицирующий запрос. Необходим для связки и обратного ответа
		/// </summary>
		/// <param name="headers">Заголовки</param>
		/// <param name="correlationId">Уникальный идентификатор запроса</param>
		/// <returns></returns>
		public static HttpHeaders AddCorrelation(this HttpHeaders headers, string correlationId = null)
		{
			headers.Add(QueueHeaders.CorrelationId, correlationId ?? Guid.NewGuid().ToString("N"));

			return headers;
		}

		/// <summary>
		/// Получить идентификатор запроса.
		/// </summary>
		/// <param name="headers">Заголовки</param>
		/// <returns></returns>
		public static string GetCorrelationHeader(this HttpHeaders headers)
		{
			headers.TryGetValues(QueueHeaders.CorrelationId, out var correlationIds);
			var correlation = correlationIds?.LastOrDefault();

			return correlation;
		}
	}
}
