using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Queue.Client.Interfaces;
using Queue.Core;
using Queue.Kafka.Client.Interfaces;

namespace Queue.Kafka.Client
{
	/// <inheritdoc cref="IKafkaResponseQueuePrepared"/> />
	public class KafkaResponseQueuePrepared : IKafkaResponseQueuePrepared, IDisposable
	{
		public static readonly HashSet<TaskStatus> Statuses = new HashSet<TaskStatus>
		{
			TaskStatus.Canceled,
			TaskStatus.Faulted,
			TaskStatus.RanToCompletion
		};
		private readonly ILogger<KafkaResponseQueuePrepared> _log;
		private readonly KafkaOptions _option;
		private readonly IHttpResponseParser _responseParser;
		private int init;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private readonly ConcurrentDictionary<string, TaskCompletionSource<HttpResponseMessage>> _tasks = new ConcurrentDictionary<string, TaskCompletionSource<HttpResponseMessage>>();


		public KafkaResponseQueuePrepared(ILogger<KafkaResponseQueuePrepared> log, KafkaOptions option, IHttpResponseParser responseParser)
		{
			_log = log;
			_option = option;
			_responseParser = responseParser;
		}

		private void Subscribe()
		{
			if (init == 1) return;

			if (_option.ConsumerConfig == null)
				throw new ArgumentNullException(nameof(_option.ConsumerConfig), nameof(KafkaOptions));

			if (Interlocked.Exchange(ref init, 1) == 1)
				return;

			try
			{
				_log.LogInformation($"Subscribe to topic {_option.ResponseTopic}");

				Task.Run(async () =>
				{
					using var consumer = new ConsumerBuilder<byte[], byte[]>(_option.ConsumerConfig).Build();
					consumer.Subscribe(_option.ResponseTopic);

					try
					{
						while (true)
						{
							var message = consumer.Consume(_cancellationTokenSource.Token);

							try
							{
								await Handle(message);
							}
							catch (Exception e)
							{
								_log.LogError(e, $"Unprocessed exception,topic: {message.Topic}, partition: {message.Partition.Value} offset: {message.Offset.Value}");
							}
						}
					}
					catch (OperationCanceledException e)
					{
						_log.LogInformation(e, "Kafka client cancelled. Close consumer");
						consumer.Close();
					}
				}, _cancellationTokenSource.Token).ContinueWith(task =>
				{
					if (task.Exception != null)
						_log.LogError(task.Exception, "Error subscribe kafka ");
					else
					{
						_log.LogInformation($"Kafka client subscribe started. {task.Status}");
					}
				}, _cancellationTokenSource.Token);
			}
			catch (Exception)
			{
				Interlocked.Exchange(ref init, 0);
				throw;
			}

		}

		public async Task<HttpResponseMessage> Prepare(string correlationId, CancellationToken token)
		{
			Subscribe();
			var cancel = new CancellationTokenSource(_option.ResponseWait);
			var responseToken = CancellationTokenSource.CreateLinkedTokenSource(token, cancel.Token);
			var waitTask = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

			_tasks.TryAdd(correlationId, waitTask);

			using (responseToken.Token.Register(() =>
			{
				_tasks.TryRemove(correlationId, out var completionSource);
				if (!Statuses.Contains(waitTask.Task.Status))
					completionSource?.TrySetCanceled();
			}))
			{
				return await waitTask.Task;
			}
		}

		public async Task Handle(ConsumeResult<byte[], byte[]> message)
		{
			var response = await _responseParser.Parse(message.Message.Value, CancellationToken.None);
			var correlationId = response.Headers.GetCorrelationHeader();

			if (string.IsNullOrWhiteSpace(correlationId))
			{
				_log.LogError($"consume response, but {QueueHeaders.ReplyId} not be set. Response will be lost");
				return;
			}

			_tasks.TryRemove(correlationId, out var taskSource);

			if (taskSource == null)
			{
				_log.LogError("Received response, but task source is lost. The Message will be removed");
				return;
			}

			taskSource.SetResult(response);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
		}
	}
}