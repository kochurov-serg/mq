using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Queue.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Client
{
	/// <inheritdoc />
	public class RabbitResponseQueuePrepared : IRabbitResponseQueuePrepared, IDisposable
	{
		public static HashSet<TaskStatus> Statuses = new HashSet<TaskStatus>
		{
			TaskStatus.Canceled,
			TaskStatus.Faulted,
			TaskStatus.RanToCompletion
		};
		private readonly ILogger<RabbitResponseQueuePrepared> _log;
		private readonly RabbitClientOptions _option;
		private readonly IRabbitConnectionFactory _connectionFactory;
		private readonly ConcurrentDictionary<string, IModel> _channels = new ConcurrentDictionary<string, IModel>();
		private readonly object objLock = new object();
		private readonly ConcurrentDictionary<string, TaskCompletionSource<HttpResponseMessage>> _tasks = new ConcurrentDictionary<string, TaskCompletionSource<HttpResponseMessage>>();
		private EventingBasicConsumer _consumer;

		public RabbitResponseQueuePrepared(ILogger<RabbitResponseQueuePrepared> log, RabbitClientOptions option,
			IRabbitConnectionFactory connectionFactory)
		{
			_log = log;
			_option = option;
			_connectionFactory = connectionFactory;
		}

		private Task Init(ConnectionFactory factory)
		{
			if (!_option.IsConfiguredClient)
				throw new ArgumentException($"{nameof(RabbitClientOptions)} must be configured {nameof(RabbitClientOptions.ClientUnique)} and {nameof(RabbitClientOptions.ClientName)}");

			var key = DefaultRabbitConnectionFactory.CreateKey(factory);

			if (_channels.TryGetValue(key, out _))
				return Task.CompletedTask;

			lock (objLock)
			{
				try
				{
					if (_channels.TryGetValue(key, out _))
						return Task.CompletedTask;

					var connection = _connectionFactory.CreateConnection(factory);

					var channel = connection.CreateModel();
					if (!_channels.TryAdd(key, channel))
					{
						throw new Exception($"Error initialize response channel {key}");
					}


					var queue = _option.ClientQueue;
					_log.LogTrace($"create consumer {nameof(EventingBasicConsumer)}");
					channel.QueueDeclare(queue, true);

					_consumer = new EventingBasicConsumer(channel);

					_consumer.Received += OnReceived;

					_consumer.Shutdown += (sender, args) =>
					{
						_log.LogCritical($"ConnectionFactory Shutdown. {args.ReplyText}");
					};

					channel.BasicConsume(queue: queue, autoAck: true, consumer: _consumer);
				}
				catch (Exception e)
				{
					_log.LogError(e, "Error create consumer response");
					if (_consumer != null)
						_consumer.Received -= OnReceived;

					_channels.TryRemove(key, out var channel);
					channel?.Dispose();
				}
				return Task.CompletedTask;
			}
		}

		public async Task<HttpResponseMessage> Prepare(string correlationId, ConnectionFactory factory,
			CancellationToken token)
		{
			await Init(factory);

			var cancel = new CancellationTokenSource(_option.ResponseWait);
			var responseToken = CancellationTokenSource.CreateLinkedTokenSource(cancel.Token);
			var waitTask = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

			_tasks.TryAdd(correlationId, waitTask);

			await using (responseToken.Token.Register(() =>
			{
				_tasks.TryRemove(correlationId, out var completionSource);
				if (!Statuses.Contains(waitTask.Task.Status))
					completionSource?.TrySetCanceled();
			}))
			{
				return await waitTask.Task;
			}
		}

		private void OnReceived(object _, BasicDeliverEventArgs args)
		{
			try
			{
				var correlationId = args.BasicProperties.CorrelationId;

				if (string.IsNullOrWhiteSpace(correlationId))
				{
					_log.LogError($"Received response, but {nameof(args.BasicProperties.CorrelationId)} is not set. Message is lost");
					return;
				}

				_tasks.TryRemove(correlationId, out var taskSource);

				if (taskSource == null)
				{
					_log.LogError("Received response, but task source is lost. The Message will be removed");
					return;
				}

				try
				{
					var statusHeaderObject = args.BasicProperties.Headers[QueueHeaders.Status];
					var status = 200;
					if (statusHeaderObject == null)
						_log.LogWarning($"No header {QueueHeaders.Status} in Response. Default status {status}");
					else
					{
						if (!(statusHeaderObject is byte[] statusHeader))
						{
							_log.LogWarning(
								$"Header {QueueHeaders.Status} in Response. Status must be string. Default status {status}");

						}
						else if (!int.TryParse(System.Text.Encoding.UTF8.GetString(statusHeader), out status))
						{
							_log.LogWarning(
								$"Header {QueueHeaders.Status} in Response. Status is incorrect. Default status {status}");
						}
					}

					var response = new HttpResponseMessage
					{
						StatusCode = (HttpStatusCode)status,
						Content = new ReadOnlyMemoryContent(args.Body)
					};

					foreach (var header in args.BasicProperties.Headers)
					{
						if (!(header.Value is byte[] value))
						{
							_log.LogWarning($"Consume header {header.Key} value must be string. Header value {header.Key} will be lost");
							continue;
						}

						if (QueueConsts.ContentHeaders.Contains(header.Key))
							response.Content.Headers.Add(header.Key, System.Text.Encoding.UTF8.GetString(value));
						else
							response.Headers.TryAddWithoutValidation(header.Key, System.Text.Encoding.UTF8.GetString(value));
					}

					taskSource.SetResult(response);
				}
				catch (Exception e)
				{
					taskSource.SetException(e);
				}
			}
			catch (Exception e)
			{
				_log.LogError(e, "Error parsing response");
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_consumer.Received -= OnReceived;
			foreach (var channel in _channels)
			{
				channel.Value?.Dispose();
			}
			_channels.Clear();
		}
	}
}