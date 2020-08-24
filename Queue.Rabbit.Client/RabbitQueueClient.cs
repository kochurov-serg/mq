using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Queue.Core;
using Queue.Rabbit.Client.Interfaces;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Options;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client
{
	/// <inheritdoc />
	public class RabbitQueueClient : IRabbitQueueClient
	{
		private bool _isDisposable;
		private readonly RabbitClientOptions _option;
		private readonly IRabbitResponseQueuePrepared _queuePrepared;
		private readonly ILogger<RabbitQueueClient> _log;
		private readonly IRabbitConnectionFactory _connectionFactory;
		public static string DefaultExchange = "";

		private Task<HttpResponseMessage> AsyncResponse => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
		{ ReasonPhrase = "Async send, response not be received", Content = new StreamContent(Stream.Null) });

		/// <inheritdoc />
		public RabbitQueueClient(RabbitClientOptions option, IRabbitResponseQueuePrepared queuePrepared, ILogger<RabbitQueueClient> log, IRabbitConnectionFactory connectionFactory, IOptions<RabbitRequestOption> opt)
		{
			_option = option ?? throw new ArgumentNullException(nameof(option));

			_queuePrepared = queuePrepared;
			_log = log;
			_connectionFactory = connectionFactory;
		}

		/// <inheritdoc cref="IRabbitQueueClient" />
		public async Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken token)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (request.RequestUri == null)
				throw new ArgumentNullException(nameof(request.RequestUri));

			var routing = request.RequestUri.Host;
			var connectioFactory = _option.Connection?.ConnectionFactory;

			if (request.Properties.TryGetValue(RabbitRequestOption.RequestProperty, out var requestObjectOption))
			{
				_log.LogTrace("Rabbit Request configured");

				if (requestObjectOption is RabbitRequestOption requestOption)
				{
					if (requestOption.ConnectionFactory != null)
					{
						connectioFactory = requestOption.ConnectionFactory;
					}

					if (requestOption.Delay != TimeSpan.Zero)
					{
						var delay = DelayOptions.CreateDelayOption(requestOption.Delay);
						routing = string.Join("/", routing, delay.Uri.ToString());
					}
				}
				else
				{
					throw new ArgumentException(
						$"Incorrect configured {nameof(HttpRequestMessage)} property {RabbitRequestOption.RequestProperty}. is not set type {nameof(RabbitRequestOption)}");
				}
			}

			if (connectioFactory == null)
				throw new ArgumentException($"ConnectionFactory in option or {nameof(request.Properties)} key name {RabbitRequestOption.RequestProperty}.{nameof(RabbitRequestOption.ConnectionFactory)} must be set");

			var connection = _connectionFactory.CreateConnection(connectioFactory);
			using var channel = connection.CreateModel();
			var props = channel.CreateBasicProperties().Prepare(request);

			if (_option.ClientName != null)
				props.AppId = _option.ClientName;

			Task<HttpResponseMessage> prepared;

			var body = new byte[0];
			if (request.Content != null)
			{
				body = await request.Content.ReadAsByteArrayAsync();
			}

			var correlationId = request.Headers.GetCorrelationHeader();
			if (correlationId != null)
			{
				if (!_option.IsConfiguredClient)
				{
					_log.LogWarning($"{nameof(_option.ClientName)} and {nameof(_option.ClientUnique)} required configure. Answer for correlationId {correlationId} not be received!");
					prepared = AsyncResponse;
				}
				else
				{
					props.CorrelationId = correlationId;
					props.ReplyToAddress = new PublicationAddress(RabbitConsts.Schema, "", _option.ClientQueue);
					prepared = _queuePrepared.Prepare(correlationId, connectioFactory, token);
				}
			}
			else
			{
				prepared = AsyncResponse;
			}

			token.ThrowIfCancellationRequested();

			_log.LogTrace("rabbit {url}. route: {queue}. request {path}, length {length} sending.... ", connection.Endpoint.ToString(), routing, request.RequestUri.Host + request.RequestUri.PathAndQuery, body.Length);
			channel.BasicPublish(DefaultExchange, routing, props, body);
			_log.LogTrace("route: {queue}. request sended", routing);

			return await prepared;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (_isDisposable)
				return;

			_isDisposable = true;
		}
	}
}
