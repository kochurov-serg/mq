using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NATS.Client;
using Queue.Client.Interfaces;
using Queue.Core;
using Queue.Nats.Client.Interfaces;
using Queue.Nats.Core;

namespace Queue.Nats.Client
{
	public class NatsQueueClient : INatsQueueClient
	{
		private readonly ILogger<NatsQueueClient> _log;
		private readonly NatsQueueClientOption _clientOption;
		private readonly INatsQueueConnection _connection;
		private readonly IHttpResponseParser _responseParser;

		public NatsQueueClient(ILogger<NatsQueueClient> log, NatsQueueClientOption clientOption, INatsQueueConnection connection, IHttpResponseParser responseParser)
		{
			_log = log;
			_clientOption = clientOption;
			_connection = connection;
			_responseParser = responseParser;
		}

        public async Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken token)
        {
            if (!request.Options.TryGetValue(new HttpRequestOptionsKey<Options>(NatsQueueClientOption.ConnectionProperty), out var options))
            {
                options = _clientOption.Options;
            }

			if (options == null)
				throw new ArgumentException(
					$"ConnectionFactory in option or {nameof(request.Options)} key name {NatsQueueClientOption.ConnectionProperty} must be set");

			var content = new HttpMessageContent(request);
			var bytes =await content.ReadAsByteArrayAsync().ConfigureAwait(false);
			var correlation = request.Headers.GetCorrelationHeader();
			var connection = _connection.CreateConnection(options);
			if (correlation != null)
			{
				var responseMsg = await connection.RequestAsync(request.RequestUri.Host, bytes, token);
				var responseMessage = await _responseParser.Parse(responseMsg.Data, token);
				return responseMessage;
			}

			connection.Publish(request.RequestUri.Host, bytes);
			_log.LogTrace("Message to {url} sended", request.RequestUri.Host);

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_connection?.Dispose();
		}
	}
}

