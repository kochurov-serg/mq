using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using Queue.Kafka.Core;

namespace Queue.Kafka.Server;

public class KafkaResponseProcessed : IKafkaResponseProcessed
{
    private readonly ILogger<KafkaResponseProcessed> _log;
    private readonly KafkaServerOption _option;

    public KafkaResponseProcessed(ILogger<KafkaResponseProcessed> log, KafkaServerOption option)
    {
        _log = log;
        _option = option;
    }

    /// <inheritdoc />
    public async Task Handle(ConsumeResult<byte[], byte[]> requestMessage, HttpContext context)
    {
        context.Request.Headers.TryGetValue(QueueHeaders.ReplyTo, out var replyTo);
        context.Request.Headers.TryGetValue(QueueHeaders.ReplyId, out var correlationId);

        if (string.IsNullOrWhiteSpace(replyTo))
        {
            _log.LogTrace($"header {QueueHeaders.ReplyTo} not set. Response not be sent.");
            return;
        }

        var message = new Message<byte[], byte[]>
        {
            Timestamp = new Timestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), TimestampType.CreateTime),
            Value = await context.Response.Body.ReadAllBytesAsync(),
            Headers = new Headers()
        };
        message.Headers.Add(HeaderNames.ContentType.ToLowerInvariant(), KafkaExtensions.HeaderValue(context.Response.ContentType));
        message.Headers.Add(HeaderNames.ContentLength.ToLowerInvariant(), KafkaExtensions.HeaderValue(message.Value.LongLength.ToString()));
        message.Headers.Add(QueueHeaders.ReplyId.ToLowerInvariant(), KafkaExtensions.HeaderValue(correlationId.ToArray()));

        foreach (var header in context.Response.Headers)
        {
            message.Headers.Add(header.Key.ToLowerInvariant(), KafkaExtensions.HeaderValue(header.Value.ToString()));
        }

        using var producer = new ProducerBuilder<byte[], byte[]>(_option.ProducerConfig).Build();
        var result = await producer.ProduceAsync(replyTo, message);
        _log.LogTrace($"Response sent to {replyTo} status: {result.Status}");
    }
}