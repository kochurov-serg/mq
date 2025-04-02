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

namespace Queue.Rabbit.Client;

/// <inheritdoc />
public class RabbitResponseQueuePrepared(ILogger<RabbitResponseQueuePrepared> log, RabbitClientOptions option, IRabbitConnectionFactory connectionFactory) : IRabbitResponseQueuePrepared, IDisposable
{
    public static HashSet<TaskStatus> Statuses = new()
    {
        TaskStatus.Canceled,
        TaskStatus.Faulted,
        TaskStatus.RanToCompletion
    };

    private readonly ConcurrentDictionary<string, IChannel> _channels = new();
    private readonly Semaphore _semaphore = new(1, 1);
    private readonly ConcurrentDictionary<string, TaskCompletionSource<HttpResponseMessage>> _tasks = new();
    private AsyncEventingBasicConsumer _consumer;

    private async Task Init(ConnectionFactory factory)
    {
        if (!option.IsConfiguredClient)
            throw new ArgumentException($"{nameof(RabbitClientOptions)} must be configured {nameof(RabbitClientOptions.ClientUnique)} and {nameof(RabbitClientOptions.ClientName)}");

        var key = DefaultRabbitConnectionFactory.CreateKey(factory);

        if (_channels.TryGetValue(key, out _))
            return;

        _semaphore.WaitOne();

        try
        {
            if (_channels.TryGetValue(key, out _))
                return;

            var connection = await connectionFactory.CreateConnection(factory).ConfigureAwait(false);

            var channel = await connection.CreateChannelAsync().ConfigureAwait(false);
            if (!_channels.TryAdd(key, channel))
            {
                throw new Exception($"Error initialize response channel {key}");
            }

            var queue = option.ClientQueue;
            log.LogTrace($"create consumer {nameof(AsyncEventingBasicConsumer)}");
            await channel.QueueDeclareAsync(queue, true).ConfigureAwait(false);

            _consumer = new AsyncEventingBasicConsumer(channel);

            _consumer.ReceivedAsync += OnReceived;

            _consumer.ShutdownAsync += (sender, args) =>
            {
                log.LogCritical($"ConnectionFactory Shutdown. {args.ReplyText}");
                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: queue, autoAck: true, consumer: _consumer).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            log.LogError(e, "Error create consumer response");
            if (_consumer != null)
                _consumer.ReceivedAsync -= OnReceived;

            _channels.TryRemove(key, out var channel);
            channel?.Dispose();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<HttpResponseMessage> Prepare(string correlationId, ConnectionFactory factory, CancellationToken token)
    {
        await Init(factory);

        var cancel = new CancellationTokenSource(option.ResponseWait);
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

    private Task OnReceived(object _, BasicDeliverEventArgs args)
    {
        try
        {
            var correlationId = args.BasicProperties.CorrelationId;

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                log.LogError($"Received response, but {nameof(args.BasicProperties.CorrelationId)} is not set. Message is lost");
                return Task.CompletedTask;
            }

            _tasks.TryRemove(correlationId, out var taskSource);

            if (taskSource == null)
            {
                log.LogError("Received response, but task source is lost. The Message will be removed");
                return Task.CompletedTask;
            }

            try
            {
                var statusHeaderObject = args.BasicProperties.Headers[QueueHeaders.Status];
                var status = 200;
                if (statusHeaderObject == null)
                    log.LogWarning($"No header {QueueHeaders.Status} in Response. Default status {status}");
                else
                {
                    if (!(statusHeaderObject is byte[] statusHeader))
                    {
                        log.LogWarning(
                            $"Header {QueueHeaders.Status} in Response. Status must be string. Default status {status}");

                    }
                    else if (!int.TryParse(System.Text.Encoding.UTF8.GetString(statusHeader), out status))
                    {
                        log.LogWarning(
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
                        log.LogWarning($"Consume header {header.Key} value must be string. Header value {header.Key} will be lost");
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
            log.LogError(e, "Error parsing response");
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _consumer.ReceivedAsync -= OnReceived;
        foreach (var channel in _channels)
        {
            channel.Value?.Dispose();
        }
        _channels.Clear();
    }
}