using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Notification.Client.Models;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Amqp.Server.RabbitMq
{
	public class RabbitAmqpCommunication : IRabbitAmqpCommunication
	{
		private readonly ILogger<RabbitAmqpCommunication> _log;
		private bool _isDisposable;
		private IConnection _connection;
		public IModel Channel { get; private set; }
		private static readonly SemaphoreSlim _lockInit = new SemaphoreSlim(1, 1);
		private readonly ConnectionFactory _factory;

		public RabbitAmqpCommunication(ILogger<RabbitAmqpCommunication> log, AmqpConnection options)
		{
			_log = log;
			_factory = options.ConnectionFactory;
		}

		/// <summary>
		/// Установка подключения. Соединение пытается установиться постоянно
		/// </summary>
		/// <returns></returns>
		public async Task Init()
		{
			await InernalInit();
		}

		private async Task InernalInit()
		{
			if (_connection != null && Channel != null)
				return;

			await _lockInit.WaitAsync();

			try
			{
				if (_connection != null && Channel != null)
					return;

				_log.LogInformation($"Подключение к RabbitMQ. {_factory.HostName} {_factory.Port} {_factory.UserName}");
				_connection = _factory.CreateConnection();
				_connection.ConnectionShutdown += (sender, args) =>
				{
					_log.LogWarning($"Соединение с RabbitMQ закрыто. {args.ReplyText}");
				};
				_log.LogInformation("Подключение к RabbitMQ установлено\nСоздание канала");
				Channel = _connection.CreateModel();

				_log.LogInformation("Канал создан");
				Channel.CallbackException += Channel_CallbackException;
				Channel.FlowControl += (sender, args) =>
				{
					_log.LogDebug("FlowControl");
				};
				Channel.ModelShutdown += (sender, args) =>
				{
					_log.LogDebug("ModelShutdown");
				};
			}
			catch (Exception e)
			{
				_log.LogError(e, "Подключение к RabbitMQ не установлено");

				throw;
			}
			finally
			{
				_lockInit.Release(1);
			}
		}

		public void DeclareParallel(string source, string queue, params string[] destinations)
		{
			Channel.ExchangeDeclare(source, "fanout", true);
			foreach (var destination in destinations)
			{
				Channel.ExchangeBindNoWait(destination, source, queue);
			}
		}

		public RabbitDeclare Declare(string exchangeName, string queueName, bool autoDelete)
		{
			var queue = AmqpCommunication.Join(exchangeName, queueName);
			_log.LogTrace("Создание exchange {exchangeName}", exchangeName);
			Channel.ExchangeDeclare(exchangeName, "direct", true);

			_log.LogTrace("Создание очереди queue {queue}", queue);

			if (!autoDelete)
			{
				Channel.BasicQos(0, 5, true);
			}

			Channel.QueueDeclare(queue, false, false, autoDelete, null);

			_log.LogTrace("Привязка очереди к exchange. {exchangeName} -> {queue}", queue, exchangeName);
			Channel.QueueBind(queue, exchangeName, queueName);
			_log.LogTrace("Очередь и exchange созданы. {exchangeName} -> {queue}", queue, exchangeName);
			return new RabbitDeclare { ExchangeName = exchangeName, QueueName = queue };
		}

		private void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
		{
			_log.LogCritical(e.Exception, $"Проблема с подключением к RabbitMQ");
		}

		public void CreateBasicConsumer(string queue, EventHandler<BasicDeliverEventArgs> received, EventHandler<ConsumerEventArgs> register)
		{

			var consumer = new EventingBasicConsumer(Channel);
			consumer.Received += received;
			consumer.Registered += async (sender, args) =>
			{
				await Init();
				register(sender, args);
			};

			consumer.Shutdown += (sender, args) =>
			{
				_log.LogCritical($"ConnectionFactory Shutdown. {args.ReplyText}");
			};
			Channel.BasicConsume(queue, false, consumer);

		}

		public async Task Send(string exchangeName, string queueName, BasicDeliverEventArgs args)
		{
			Channel.BasicPublish(exchangeName, queueName, args.BasicProperties, args.Body);
		}

		public async Task SendError(BasicDeliverEventArgs args, int countRetry, string queueName)
		{
			var retry = args.BasicProperties.IncrementRetry();

			if (retry < countRetry)
			{
				try
				{
					Channel.BasicPublish(args.Exchange, queueName, args.BasicProperties, args.Body);
					Channel.BasicReject(args.DeliveryTag, false);
				}
				catch (Exception e)
				{
					_log.LogError(e, "Ошибка отправки сообщения с ошибкой");
				}

			}
		}

		public void Dispose()
		{
			if (_isDisposable)
				return;

			DisposeChannel(Channel);
			_connection?.Dispose();
			_isDisposable = true;
		}

		private void DisposeChannel(IModel channel)
		{
			if (channel != null)
			{
				if (!channel.IsClosed)
					channel.Close();

				channel.Dispose();
			}
		}
	}
}
