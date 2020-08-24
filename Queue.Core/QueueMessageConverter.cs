using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Queue.Core.Intefaces;

namespace Queue.Core
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
					request.Headers.TryAddToLowerCase(header.Key, header.Value);
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

		//public async Task<HttpResponseMessage> ToMessage(QueueMessageResponse message)
		//{
		//	var response = new HttpResponseMessage()
		//	{
				
		//	};MessageContentHttpMessageSerializer

		//	response.Content= new HttpMessageContent();
		//	foreach (var header in message.Headers)
		//	{
		//		response.Headers.TryAddWithoutValidation(header.Key, header.Value);
		//	}
			
		//}
	}
}