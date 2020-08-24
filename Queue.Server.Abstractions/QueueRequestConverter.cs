using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using Queue.Server.Abstractions.Interfaces;

namespace Queue.Server.Abstractions
{
	public class QueueRequestConverter : IQueueRequestConverter
	{
		private readonly ILogger<QueueRequestConverter> _log;

		public QueueRequestConverter(ILogger<QueueRequestConverter> log)
		{
			_log = log;
		}

		public async Task<QueueRequest> Convert(byte[] bytes)
		{
			var request = await Convert(new MemoryStream(bytes));

			return request;
		}

		public async Task<QueueRequest> Convert(Stream stream)
		{
			var requestMsg = new HttpRequestMessage
			{
				Content = new StreamContent(stream)
				{
					Headers = { { HeaderNames.ContentType, "application/http; msgtype=request" } }
				}
			};

			try
			{
				var httpRequest = await requestMsg.Content.ReadAsHttpRequestMessageAsync();
				var request = await Convert(httpRequest);
				return request;
			}
			catch (Exception e)
			{
				if (stream.CanSeek)
					stream.Position = 0;

				_log.LogError(e, $"Cannot convert request from {await requestMsg.Content.ReadAsStringAsync()} to HttpRequestMessage");
			}

			return null;
		}

		private void AddHeaders(IHeaderDictionary destination, HttpHeaders source)
		{
			foreach (var header in source)
			{
				destination.TryAdd(header.Key, new StringValues(header.Value != null ? header.Value.ToArray() : new[] { string.Empty }));
			}
		}

		public async Task<QueueRequest> Convert(HttpRequestMessage message)
		{
			var emptyBody = message.Method == HttpMethod.Get || message.Method == HttpMethod.Delete;

			if (emptyBody && message.Content != null)
			{
				_log.LogWarning($"uri: {message.RequestUri} method not available body. But body is set");
			}

			var query = message.RequestUri.ParseQueryString();

			var request = new QueueRequest
			{
				Body = !emptyBody ? await message.Content.ReadAsStreamAsync() : Stream.Null,
				ContentType = message.Content?.Headers?.ContentType?.ToString() ?? QueueConsts.DefaultContentType,
				IsHttps = false,
				Method = message.Method.ToString(),
				Protocol = message.RequestUri.Scheme,
				QueryString = new QueryString(message.RequestUri.Query),
				Query = new QueryCollection(query.AllKeys.GroupBy(x => x).ToDictionary(x => x.Key, x => new StringValues(query[x.Key]))),
				Path = message.RequestUri.AbsolutePath,
				Host = new HostString(message.RequestUri.Host),
				Scheme = message.RequestUri.Scheme,
				PathBase = PathString.Empty
			};
			request.ContentLength = request.Body.Length;
			AddHeaders(request.Headers, message.Headers);
			if (message.Content?.Headers != null)
				AddHeaders(request.Headers, message.Content.Headers);

			return request;
		}
	}
}