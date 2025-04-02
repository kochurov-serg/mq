using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Queue.Rabbit.Client.Interfaces;
using Queue.Rabbit.Core.Options;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client;

public class RabbitMessageHandler : HttpMessageHandler
{
    private readonly IRabbitQueueClient _client;

    public ConnectionFactory Factory { get; set; }

    public RabbitMessageHandler(IRabbitQueueClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (Factory != null)
        {
            var propertyKey = new HttpRequestOptionsKey<RabbitRequestOption>(RabbitRequestOption.RequestProperty);

            if (!request.Options.TryGetValue(propertyKey, out var requestOption))
            {
                requestOption = new RabbitRequestOption();
                request.Options.Set(propertyKey, requestOption);
            }

            requestOption.ConnectionFactory = Factory;
        }

        var response = await _client.Send(request, cancellationToken);

        return response;
    }
}