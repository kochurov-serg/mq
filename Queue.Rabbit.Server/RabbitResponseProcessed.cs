using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Rabbit.Server.Extensions;
using Queue.Rabbit.Server.Interfaces;
using Queue.Rabbit.Server.Repeat;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server;

/// <inheritdoc />
public class RabbitResponseProcessed(ILogger<RabbitResponseProcessed> log, IRabbitCommunicationServer communication)
    : IRabbitResponseProcessed
{
    public async Task Handle(BasicDeliverEventArgs requestArgs, HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;

        var requestProps = requestArgs.BasicProperties;

        log.LogInformation("{route} response status {status}", requestArgs.RoutingKey, response.StatusCode);

        if (response.StatusCode is 500 or 504)
        {
            log.LogInformation("Response Status {status}. Retry", response.StatusCode);

            var config = response.Headers.ParseRepeat() ?? request.Headers.ParseRepeat();

            if (config != null)
            {
                await communication.Retry(requestArgs.Exchange, new BasicProperties(requestProps), requestArgs.Body, config, requestArgs.DeliveryTag, CancellationToken.None);
                return;
            }
        }

        if (requestProps.ReplyTo == null)
        {
            return;
        }

        var address = requestProps.ReplyToAddress;

        if (address == null)
        {
            log.LogError("BasicProperties property ReplyToAddress unset");
            return;
        }

        var body = ReadOnlyMemory<byte>.Empty;

        if (response.Body != Stream.Null && response.Body.Length > 0)
        {
            body = new ReadOnlyMemory<byte>(await response.Body.ReadAllBytesAsync());
        }

        log.LogTrace("Response. exchange: {exchange} route: {queue}", address.ExchangeName, address.RoutingKey);

        try
        {
            var basicProperties =
                new BasicProperties(requestArgs.BasicProperties).CreateBasicPropertiesResponse(context.Request,
                    response);

            await communication
                .Send(address.ExchangeName, address.RoutingKey, basicProperties, body, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            log.LogError(e, $"Error send message exchange: {address.ExchangeName}, routing {address.RoutingKey}");
        }

        log.LogTrace("Response. exchange: {exchange} route: {queue} sended", address.ExchangeName, address.RoutingKey);
    }
}