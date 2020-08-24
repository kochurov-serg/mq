using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Notification.Amqp.Client.Abstractions;
using Notification.Amqp.Client.Abstractions.Interfaces;
using Notification.Amqp.Extensions;
using Notification.Amqp.Server.Abstractions;
using Notification.Amqp.Server.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Amqp.Client.RabbitMq
{
	/// <summary>
	/// Клиент реализующий общение между сервисов посредством протокола amqp
	/// </summary>
	public class RabbitAmqpClient : IAmqpClient
	{
		private bool _isDisposable;
		private readonly AmqpClientOptions _option;
		private readonly ILogger<RabbitAmqpClient> _log;
		private readonly IRabbitMessageConverter _converter;
		private readonly IRabbitAmqpCommunication _communication;
		private readonly string _exchange;

		/// <inheritdoc />
		public RabbitAmqpClient(AmqpClientOptions option, ILogger<RabbitAmqpClient> log, IRabbitMessageConverter converter, IRabbitAmqpCommunication communication)
		{
			_option = option ?? throw new ArgumentNullException(nameof(option), "Настройки отправки сообщений в RabbitMQ не заданы");
			_log = log;
			_converter = converter;
			_communication = communication;
			_exchange = _option.ClientName;
		}

		/// <inheritdoc />
		public async Task Send(Uri baseUri, HttpRequestMessage request, AmqpRequestOptions options)
		{
			var props = _communication.Channel
				.CreateBasicProperties()
				.Prepare(request, baseUri);

			props.AppId = _option.ClientName;
			var correlationId = request.Headers.GetCorrelationHeader();
			if (correlationId != null)
			{
				props.CorrelationId = correlationId;
				props.ReplyToAddress = new PublicationAddress(AmqpCommunication.ProtocolName, _exchange, AmqpCommunication.ClientResponseQueue);
			}

			var body = await request.Content.ReadAsByteArrayAsync();

			var routing = AmqpCommunication.ResourceRequestQueue;
			_log.LogTrace("Response. exchange: {exchange} route: {queue}", baseUri.Host, routing);
			if (options.CancellationToken.IsCancellationRequested)
				return;

			await _communication.Send(baseUri.Host, routing, new BasicDeliverEventArgs { Body = body, BasicProperties = props });
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (_isDisposable)
				return;

			_communication.Dispose();
			_isDisposable = true;
		}

		/// <inheritdoc />
		public async Task Subscribe(Func<HttpResponseMessage, bool> callback)
		{
			await _communication.Init();
			var declare = _communication.Declare(_exchange, AmqpCommunication.ClientResponseQueue, false);
			_communication.Declare(_exchange, AmqpCommunication.ClientDelayQueue, false);
			_communication.Declare(_exchange, AmqpCommunication.ClientErrorQueue, false);

			_communication.CreateBasicConsumer(declare.QueueName, async (sender, args) =>
			 {
				 try
				 {
					 _log.LogTrace($"Queue: {declare.QueueName}. CorrelationId {args.BasicProperties.CorrelationId}. Message size {args.Body.LongLength}");

					 var correlationId = args.BasicProperties.CorrelationId;

					 if (string.IsNullOrWhiteSpace(correlationId))
					 {
						 _log.LogError($"Correlation not found. This request not be processed. Please set header {AmqpHeaders.CorrelationId}. Unique value");
						 SendError(args);
						 return;
					 }

					 _log.LogTrace($"Parsing response {correlationId}");
					 var response = await _converter.TryParse(args);

					 if (response == null)
					 {
						 _log.LogTrace($"Parsing error {correlationId}. Send error queue");
						 SendError(args);
					 }

					 callback(response);

					 _communication.Channel.BasicAck(args.DeliveryTag, false);
					 _log.LogTrace($"{correlationId}: Operation success");
				 }
				 catch (Exception e)
				 {
					 _log.LogError(e, $"Exception by processing amqp message");
					 SendError(args);
				 }
			 }, (sender, args) =>
			 {
				 _log.LogInformation("client consumer register");
			 });
		}

		private void SendError(BasicDeliverEventArgs args)
		{
			_log.LogTrace($"CorrelationId {args.BasicProperties.CorrelationId ?? "-"}. Send error queue");
			_communication.SendError(args, _option.CountRetry, AmqpCommunication.ClientErrorQueue);
			if (args.DeliveryTag != 0)
			{
				_communication.Channel.BasicAck(args.DeliveryTag, false);
			}
		}
	}
}
