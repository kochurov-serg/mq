using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Queue.Client.Interfaces;

namespace Queue.Client
{
	/// <inheritdoc />
	public class HttpResponseParser : IHttpResponseParser
	{
		private readonly ILogger<HttpResponseParser> _log;

		public HttpResponseParser(ILogger<HttpResponseParser> log)
		{
			_log = log;
		}

		public async Task<HttpResponseMessage> Parse(byte[] bytes, CancellationToken token)
		{
			try
			{
				var response = new HttpResponseMessage
				{
					Content = new StreamContent(new MemoryStream(bytes))
					{
						Headers = { { HeaderNames.ContentType, "application/http; msgtype=response" } }
					}
				};

				var responseMessage = await response.Content.ReadAsHttpResponseMessageAsync(token);

				return responseMessage;
			}
			catch (Exception e)
			{
				_log.LogError(e, "Error parsing response");

				throw;
			}
		}
	}
}