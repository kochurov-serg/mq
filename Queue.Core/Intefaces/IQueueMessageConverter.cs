using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Queue.Core.Intefaces
{
	public class QueueMessageConverter : IQueueMessageConverter
	{
		private readonly ILogger<QueueMessageConverter> _log;

		public QueueMessageConverter(ILogger<QueueMessageConverter> log)
		{
			_log = log;
		}

		public async Task<QueueMessageRequest> FromRequest(HttpRequestMessage message)
		{
			_log.LogTrace($"Converting {nameof(HttpRequestMessage)} to {nameof(QueueMessageRequest)}");

			var request = new QueueMessageRequest
			{
				Uri = message.RequestUri
			};
			request.Headers = request.Headers;

			if (message.Content != null)
			{
				var streamTask = message.Content.ReadAsStreamAsync();

				_log.LogTrace($"Add headers");
				foreach (var header in message.Content.Headers)
				{
					request.Headers.TryAdd(header.Key, header.Value);
				}

				_log.LogTrace($"Converting content");
				request.Body = await streamTask;
			}

			else
			{
				request.Body = Stream.Null;
			}

			return request;
		}
	}

	/// <summary>
	/// Converting queue message
	/// </summary>
	public interface IQueueMessageConverter
	{
		/// <summary>
		/// Converting HttpRequestMessage to QueueMessage
		/// </summary>
		/// <param name="message">message</param>
		/// <returns>QueueMessage</returns>
		Task<QueueMessageRequest> FromRequest(HttpRequestMessage message);
	}
}
