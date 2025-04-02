using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Queue.Core.Intefaces;

namespace Queue.Core;

public class QueueMessageConverter(ILogger<QueueMessageConverter> log) : IQueueMessageConverter
{
    public async Task<QueueMessageRequest> FromRequest(HttpRequestMessage message)
    {
        log.LogTrace($"Converting {nameof(HttpRequestMessage)} to {nameof(QueueMessageRequest)}");

        var request = new QueueMessageRequest
        {
            Uri = message.RequestUri
        };
        request.Headers = request.Headers;

        if (message.Content != null)
        {
            var streamTask = message.Content.ReadAsStreamAsync();

            log.LogTrace($"Add headers");
            foreach (var header in message.Content.Headers)
            {
                request.Headers.TryAddToLowerCase(header.Key, header.Value);
            }

            log.LogTrace($"Converting content");
            request.Body = await streamTask;
        }

        else
        {
            request.Body = Stream.Null;
        }

        return request;
    }
}