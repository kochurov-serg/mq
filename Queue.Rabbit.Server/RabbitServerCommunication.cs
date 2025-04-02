using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Interfaces;
using Queue.Rabbit.Core.Repeat;
using Queue.Rabbit.Server.Extensions;
using Queue.Rabbit.Server.Interfaces;
using Queue.Rabbit.Server.Repeat;
using Queue.Server.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server;

/// <inheritdoc />
public class RabbitServerCommunication : IRabbitCommunicationServer
{
    private readonly ILogger<RabbitServerCommunication> _log;
    private readonly IRabbitmqConnection _connection;
    private readonly RabbitServerOptions _option;
    private IChannel _channel;
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

    private async Task CreateDelayQueue(string queueName, long ttl)
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

        await _channel.QueueDeclareAsync(queueName, true, false, false, props);
    }

    public async Task Init()
    {
        _delayConfig.Init(_option.Queue.Uri, _option.DelayOptions);

        _channel = await _connection.CreateModel();

        try
        {
            _log.LogTrace("set basic qos {qos}", _option.Qos);
            await _channel.BasicQosAsync(_option.Qos.PrefetchSize, _option.Qos.PrefetchCount, _option.Qos.Global).ConfigureAwait(false);

            Dictionary<string, object> queueArgs = new Dictionary<string, object>
            {
                {Headers.XExpires, _option.Queue.Expires }
            };

            if (_delayQueue != null)
            {
                queueArgs.TryAdd(Headers.XDeadLetterExchange, _exchange, false);
                queueArgs.TryAdd(Headers.XDeadLetterRoutingKey, _delayQueue, false);
                await CreateDelayQueue(_delayQueue, _option.DefaultDelayQueue.MessageTtl).ConfigureAwait(false);
                if (_option.DelayOptions?.QueueOptions != null)
                {
                    foreach (var queue in _delayConfig.Queues)
                    {
                        await CreateDelayQueue(queue.QueueName, queue.Option.MessageTtl).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _log.LogTrace("delay queue not set");
            }

            _log.LogTrace("create queue {queue}", _queue);
            await _channel.QueueDeclareAsync(_queue, true, false, false, queueArgs).ConfigureAwait(false);

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

                await _channel.QueueDeclareAsync(_errorQueue, true, false, false, props).ConfigureAwait(false);

                _channel.CallbackExceptionAsync += Channel_CallbackException;
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
    }

    private Task Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
    {
        _log.LogCritical(e.Exception, "RabbitMQ Channel exception");
        return Task.CompletedTask;
    }

    public async Task CreateBasicConsumer(AsyncEventHandler<BasicDeliverEventArgs> received)
    {
        if (_option.Consumer == null || _option.Consumer.ParallelCount == 0)
        {
            _log.LogInformation($"Rabbit consumer 0. {nameof(RabbitServerOptions)} property {nameof(_option.Consumer.ParallelCount)}");
            return;
        }

        for (int i = 0; i < _option.Consumer.ParallelCount; i++)
        {
            _log.LogTrace($"create consumer {nameof(AsyncEventingBasicConsumer)}");
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += received;

            consumer.ShutdownAsync += ShutdownAsync;
            await _channel.BasicConsumeAsync(_queue, _option.Queue.AutoAck, _option.Consumer.ConsumerTag, consumer);
        }
        
    }

    private Task ShutdownAsync(object sender, ShutdownEventArgs args )
    {
        _log.LogCritical($"ConnectionFactory Shutdown. {args.ReplyText}");
        return Task.CompletedTask;
    }

    public async Task Send(string exchange, string routingKey, BasicProperties basicProperties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        try
        {
            await using var channel = await _connection.CreateModel().ConfigureAwait(false);

            _log.LogTrace("send message {exchange} {queue} body length: {bodyLength}", exchange, routingKey, body.Length);
            await channel.BasicPublishAsync(exchange, routingKey, true, basicProperties, body, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _log.LogError(e, "Exchange: {exchange}, queue: {queue}, body length: {bodyLength}. Fail send", exchange, routingKey, body.Length);

            throw;
        }
    }

    public async Task Ack(ulong deliveryTag)
    {
        _log.LogTrace($"Ack {deliveryTag}");
        await _channel.BasicAckAsync(deliveryTag, false);
    }

    public async Task TryAck(ulong deliveryTag)
    {
        try
        {
            await Ack(deliveryTag);
        }
        catch (Exception e)
        {
            _log.LogError(e, $"Error ack message {deliveryTag}");
        }
    }

    public async Task<bool> Retry(BasicDeliverEventArgs args, QueueContext context)
    {
        if (context == null)
        {
            _log.LogError("queue context is null");
            return false;
        }

        if (context.RequestAborted.IsCancellationRequested)
        {
            _log.LogTrace("context cancelled. Retry not need");
            return false;
        }

        try
        {
            var request = context.Request;
            var response = context.Response;

            var config = response.Headers.ParseRepeat() ?? request.Headers.ParseRepeat();

            if (config == null)
            {
                _log.LogInformation("Retry config in header not found");
                return false;
            }

            var basicProperties = new BasicProperties(args.BasicProperties);
            var result = await Retry(args.Exchange, basicProperties, args.Body, config, args.DeliveryTag, CancellationToken.None);

            return result;
        }
        catch (Exception e)
        {
            _log.LogError(e, "Retry error");
        }

        return false;
    }

    public async Task<bool> Retry(string exchange, BasicProperties basicProperties, ReadOnlyMemory<byte> body, RepeatConfig config, ulong deliveryTag, CancellationToken cancellationToken)
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

        if (config.Count > 0)
        {
            basicProperties.AddRetry(config, delay);
            await Send(exchange, delay.QueueName, basicProperties, body, cancellationToken);
            _log.LogTrace("message send retry");
            return true;
        }

        _log.LogTrace("Count retry exceeded");

        return false;
    }

    public async Task SendException(BasicDeliverEventArgs args)
    {
        if (args.Redelivered)
        {
            _log.LogTrace($"DeliveryTag {args.DeliveryTag} already be delay queue.");
            await Ack(args.DeliveryTag);
        }
        else if (_delayQueue != null)
        {
            await _channel.BasicRejectAsync(args.DeliveryTag, false);
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