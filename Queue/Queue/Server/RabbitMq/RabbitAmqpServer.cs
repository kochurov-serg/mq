using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notification.Amqp.Extensions;
using Notification.Amqp.Server.Abstractions.Interfaces;
using Notification.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Amqp.Server.RabbitMq
{
	/// <summary>
	/// Сервер обработки запросов от Rabbit
	/// </summary>
	public class RabbitAmqpServer : IAmqpServer
	{
		private bool _isDisposable = false;
		private readonly AmqpServerOptions _option;
		private readonly ILogger<RabbitNotificationStorage> _log;
		private readonly IAmqpConverter<BasicDeliverEventArgs> _converter;
		private readonly IRabbitAmqpCommunication _communication;

		/// <inheritdoc />
		public IFeatureCollection Features { get; }

		/// <inheritdoc />
		public RabbitAmqpServer(AmqpServerOptions option, ILogger<RabbitNotificationStorage> log, IAmqpConverter<BasicDeliverEventArgs> converter, IRabbitAmqpCommunication communication)
		{
			_log = log;
			Features = new FeatureCollection();
			_converter = converter;
			_communication = communication;
			_option = option ?? throw new ArgumentNullException(nameof(option), "Настройки отправки сообщений в RabbitMQ не заданы");
		}

		internal async Task SendResponse(BasicDeliverEventArgs request, HttpResponse response)
		{
			var requestProps = request.BasicProperties;
			var replyToAdrress = requestProps.ReplyToAddress;

			if (replyToAdrress == null)
			{
				_log.LogInformation($"Request {requestProps.MessageId} ReplyToAddress is empty. Response not be sent");
				return;
			}

			var props = _communication.Channel.CreateBasicProperties().CreateBasicPropertiesResponse(requestProps, response);
			props.AppId = _option.ServerName.Host;
			var body = await response.Body.ReadAllBytesAsync();
			var routing = replyToAdrress.RoutingKey;

			_log.LogTrace("Response. exchange: {exchange} route: {queue}", replyToAdrress.ExchangeName, replyToAdrress.RoutingKey);

			try
			{
				await _communication.Send(replyToAdrress.ExchangeName, routing, new BasicDeliverEventArgs { Body = body, BasicProperties = props });
			}
			catch (Exception e)
			{
				_log.LogError(e, "Ошибка при отправке сообщения");

			}
			_log.LogTrace("Response. exchange: {exchange} route: {queue} sended", replyToAdrress.ExchangeName, routing);

		}

		public void Dispose()
		{
			if (_isDisposable)
				return;

			_communication.Dispose();
			_isDisposable = true;
		}

		/// <inheritdoc />
		public async Task StartAsync(RequestDelegate requestDelegate, CancellationToken cancellationToken)
		{
			var host = _option.ServerName.Host;
			await _communication.Init();
			var declare = _communication.Declare(host, AmqpCommunication.ResourceRequestQueue, false);
			_communication.Declare(host, AmqpCommunication.ResourceDelayQueue, false);
			_communication.Declare(host, AmqpCommunication.ResourceErrorQueue, false);

			_communication.CreateBasicConsumer(declare.QueueName, async (sender, args) =>
			{
				try
				{
					_log.LogTrace("Очередь {queueName}. Обработка запроса", declare.QueueName);
					var context = await _converter.Parse(args, Features);

					if (context != null)
					{
						await requestDelegate(context);
						await SendResponse(args, context.Response);
						_communication.Channel.BasicAck(args.DeliveryTag, false);
					}
					else
					{
						SendError(args);
					}
				}
				catch (Exception e)
				{
					// Добавить отправку сообщения в очередь ожидания
					_log.LogError(e, $"Ошибка при обработке сообщения");
					SendError(args);
				}
			}, (sender, args) =>
			 {
				 _log.LogInformation($"Consumer {args.ConsumerTag} Registered");
			 });
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			Dispose();
			return Task.CompletedTask;
		}

		public void SendError(BasicDeliverEventArgs args) => _communication.SendError(args, _option.CountRetry, AmqpCommunication.ResourceErrorQueue);
	}
}