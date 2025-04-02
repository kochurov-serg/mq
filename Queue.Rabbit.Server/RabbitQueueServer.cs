using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Rabbit.Server.Interfaces;
using Queue.Server.Abstractions;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server;

/// <summary>
/// Сервер обработки запросов от Rabbit
/// </summary>
public class RabbitQueueServer : IRabbitQueueServer
{
    private bool _isDisposable;
    private readonly ILogger<RabbitQueueServer> _log;
    private readonly IQueueConverter<BasicDeliverEventArgs> _converter;
    private readonly IRabbitCommunicationServer _communication;
    private readonly IRabbitResponseProcessed _responseProcessed;


    /// <inheritdoc />
    public RabbitQueueServer(ILogger<RabbitQueueServer> log, IQueueConverter<BasicDeliverEventArgs> converter, IRabbitCommunicationServer communication, IRabbitResponseProcessed responseProcessed)
    {
        _log = log;
        _converter = converter;
        _communication = communication;
        _responseProcessed = responseProcessed;
    }

    /// <inheritdoc />
    public async Task Start(RequestDelegate requestDelegate, CancellationToken cancellationToken)
    {
        await _communication.Init();

        await Consume(requestDelegate, cancellationToken);
    }

    private async Task Consume(RequestDelegate requestDelegate, CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();
        await _communication.CreateBasicConsumer(async (_, args) =>
            {
                QueueContext context = null;
                try
                {
                    _log.LogTrace("incoming request");
                    stopwatch.Restart();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        _log.LogTrace("Cancellation token call. {queueName}", args.RoutingKey);
                        _communication.Dispose();
                        return;
                    }

                    _log.LogTrace("Parsing request");
                    var features = new FeatureCollection();
                    features.Set(new ItemsFeature());
                    context = await _converter.Parse(args, features).ConfigureAwait(false);

                    if (context == null)
                    {
                        return;
                    }

                    await requestDelegate(context).ConfigureAwait(false);
                    await _responseProcessed.Handle(args, context).ConfigureAwait(false);
                }
                catch (NotImplementedException e)
                {
                    _log.LogError(e, "Not implemented");
                }
                catch (OperationCanceledException e)
                {
                    _log.LogError(e, "Operation cancelled");
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error processing message");
                    await _communication.Retry(args, context).ConfigureAwait(false);
                }
                finally
                {
                    await _communication.TryAck(args.DeliveryTag).ConfigureAwait(false);
                    stopwatch.Stop();
                    var elapsed = stopwatch.ElapsedMilliseconds;
                    _log.LogTrace($"Rabbit request elapsed {elapsed} ms");
                }
            });
    }

    /// <inheritdoc />
    public Task Stop(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_isDisposable)
            return;

        _communication.Dispose();
        _isDisposable = true;
    }
}