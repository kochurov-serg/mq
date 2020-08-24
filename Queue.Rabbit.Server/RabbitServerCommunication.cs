using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Interfaces;
using Queue.Rabbit.Core.Repeat;
using Queue.Rabbit.Server.Extensions;
using Queue.Rabbit.Server.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server
{
	/// <inheritdoc />
	public class RabbitServerCommunication : IRabbitCommunicationServer
	{
		private readonly ILogger<RabbitServerCommunication> _log;
		private readonly IRabbitmqConnection _connection;
		private readonly RabbitServerOptions _option;
		private IModel _channel;
		private readonly string _queue;
		private readonly string _errorQueue;
		private readonly string _delayQueue;
		private bool _isDisposable;
		private const string _exchange = "";
		private IRabbitDelayConfig _delayConfig;

		public RabbitServerCommunication(
			ILogger<RabbitServerCommunication> log,
			IRabbitmqConnection connection,
			RabbitServerOptions option, IRabbitDelayConfig delayConfig)
		{
			_log = log;
			_connection = connection;
			_option = option ?? throw new ArgumentNullException(nameof(option), "RabbitMQ settings not set.");
			_delayConfig = delayConfig;
			_queue = _option.Queue.Uri.Host;
			_errorQueue = _option.ErrorQueue == null ? null : QueueNameExtensions.GetQueue(_option.Queue.Uri, _option.ErrorQueue.Uri);
			_delayQueue = _option.DefaultDelayQueue == null
				? null
				: QueueNameExtensions.GetQueue(_option.Queue.Uri, _option.DefaultDelayQueue.Uri);
		}

		private void CreateDelayQueue(string queueName, long ttl)
		{
			_log.LogTrace($"create queue {queueName}");

			var props = new Dictionary<string, object>
			{
				{Headers.XDeadLetterExchange, _exchange},
				{Headers.XDeadLetterRoutingKey, _queue},
				{Headers.XMessageTTL, ttl}
			};

			if (_option.DefaultDelayQueue.Expires != 0)
			{
				props.Add(Headers.XExpires, _option.DefaultDelayQueue.Expires);
			}

			_channel.QueueDeclare(queueName, true, false, false, props);
		}

		public IBasicProperties CreateBasicProperties() => _channel.CreateBasicProperties();

		public Task Init()
		{
			_delayConfig.Init(_option.Queue.Uri, _option.DelayOptions);

			_channel = _connection.CreateModel();
			
			try
			{
				_log.LogTrace("set basic qos {qos}", _option.Qos);
				_channel.BasicQos(_option.Qos.PrefetchSize, _option.Qos.PrefetchCount, _option.Qos.Global);

				Dictionary<string, object> queueArgs = new Dictionary<string, object>
				{
					{Headers.XExpires, _option.Queue.Expires }
				};

				if (_delayQueue != null)
				{
					queueArgs.TryAdd(Headers.XDeadLetterExchange, _exchange, false);
					queueArgs.TryAdd(Headers.XDeadLetterRoutingKey, _delayQueue, false);
					CreateDelayQueue(_delayQueue, _option.DefaultDelayQueue.MessageTtl);
					if (_option.DelayOptions?.QueueOptions != null)
					{
						foreach (var queue in _delayConfig.Queues)
						{
							CreateDelayQueue(queue.QueueName, queue.Option.MessageTtl);
						}
					}
				}
				else
				{
					_log.LogTrace("delay queue not set");
				}

				_log.LogTrace("create queue {queue}", _queue);
				_channel.QueueDeclare(_queue, true, false, false, queueArgs);

				if (_errorQueue != null)
				{
					_log.LogTrace("create queue {queue}", _errorQueue);

					var props = new Dictionary<string, object>
					{
						{Headers.XMessageTTL, _option.ErrorQueue.MessageTtl}
					};

					if (_option.Queue.Expires != 0)
					{
						props.Add(Headers.XExpires, _option.Queue.Expires);
					}

					_channel.QueueDeclare(_errorQueue, true, false, false, props);

					_channel.CallbackException += Channel_CallbackException;
				}
				else
				{
					_log.LogInformation("error queue not set");
				}
			}
			catch (Exception e)
			{
				_log.LogError(e, $"Check exists parameters queues {string.Join(",", _queue, _errorQueue, _delayQueue)}");
				throw;
			}

			return Task.CompletedTask;
		}

		private void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
		{
			_log.LogCritical(e.Exception, "RabbitMQ Channel exception");
		}

		public void CreateBasicConsumer(EventHandler<BasicDeliverEventArgs> received)
		{
			_log.LogTrace($"create consumer {nameof(EventingBasicConsumer)}");
			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += received;

			consumer.Shutdown += (sender, args) =>
			{
				_log.LogCritical($"ConnectionFactory Shutdown. {args.ReplyText}");

			};
			_channel.BasicConsume(_queue, _option.Queue.AutoAck, consumer);
		}

		public Task Send(BasicDeliverEventArgs args)
		{
			IModel channel;
			try
			{
				using (channel = _connection.CreateModel())
				{
					args.BasicProperties.AppId = _queue;

					_log.LogTrace("send message {exchange} {queue} body length: {bodyLength}", args.Exchange, args.RoutingKey, args.Body.Length);
					channel.BasicPublish(args.Exchange, args.RoutingKey, args.BasicProperties, args.Body);

					return Task.CompletedTask;
				}
			}
			catch (Exception e)
			{
				_log.LogError(e, "Exchange: {exchange}, queue: {queue}, body length: {bodyLength}. Fail send", args.Exchange, args.RoutingKey, args.Body.Length);

				throw;
			}
		}

		public async Task SendError(BasicDeliverEventArgs args)
		{
			args.BasicProperties.AppId = _queue;

			if (_errorQueue == null)
			{
				var headersLog = args.BasicProperties.Headers.Aggregate(new StringBuilder(),
					(builder, pair) => builder.Append(pair.Key).Append(pair.Value));
				_log.LogInformation("Error queue not be configured. Message CorrelationId: {CorrelationId}, {queue} {headers} be lost",
					args.BasicProperties.CorrelationId, args.RoutingKey, headersLog);

				return;
			}

			args.RoutingKey = _errorQueue;
			args.Exchange = _exchange;
			await Send(args);
		}

		public Task Ack(BasicDeliverEventArgs args)
		{
			_log.LogTrace($"Ack {args.DeliveryTag}");
			_channel.BasicAck(args.DeliveryTag, false);
			return Task.CompletedTask;
		}

		public async Task<bool> Retry(BasicDeliverEventArgs args, RepeatConfig config)
		{
			if (config == null)
			{
				return false;
			}

			var delay = _delayConfig.GetDelay(config.Delay);

			if (delay == null)
			{
				_log.LogTrace("Delays not configured. Message go to default delay");

				return false;
			}

			args.RoutingKey = delay.QueueName;

			if (config.Count > 0)
			{
				args.BasicProperties.AddRetry(config, delay);
				await Send(args);
				await Ack(args);

				return true;
			}

			args.BasicProperties.Headers.Remove(RepeatConfig.RepeatCount);

			return false;
		}

		public async Task SendException(BasicDeliverEventArgs args)
		{

			if (!_option.RetryForever && args.Redelivered)
			{
				_log.LogTrace($"DeliveryTag {args.DeliveryTag} already be delay queue.");
				await SendError(args);
				await Ack(args);
			}
			else if (_delayQueue != null)
			{
				_channel.BasicReject(args.DeliveryTag, false);
			}
		}

		public void Dispose()
		{
			if (_isDisposable)
				return;

			_channel?.Dispose();
			_connection?.Dispose();
			_isDisposable = true;
		}
	}
}
