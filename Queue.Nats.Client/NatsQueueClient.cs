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
			var options = request.Properties.ContainsKey(NatsQueueClientOption.ConnectionProperty)
				? request.Properties[NatsQueueClientOption.ConnectionProperty] as Options
				: _clientOption.Options;

			if (options == null)
				throw new ArgumentException(
					$"ConnectionFactory in option or {nameof(request.Properties)} key name {NatsQueueClientOption.ConnectionProperty} must be set");

			var content = new HttpMessageContent(request);
			var bytesTask = content.ReadAsByteArrayAsync();
			var correlation = request.Headers.GetCorrelationHeader();
			var connection = _connection.CreateConnection(options);
			if (correlation != null)
			{
				var responseMsg = await connection.RequestAsync(request.RequestUri.Host, await bytesTask, token);
				var responseMessage = await _responseParser.Parse(responseMsg.Data, token);
				return responseMessage;
			}

			var bytes = await bytesTask;
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

