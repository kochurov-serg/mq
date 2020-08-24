using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Queue.Nats.Client.Interfaces;

namespace Queue.Nats.Client
{
	public class NatsMessageHandler : HttpMessageHandler
	{
		private readonly INatsQueueClient _client;

		public NatsMessageHandler(INatsQueueClient client)
		{
			_client = client;
		}

		/// <inheritdoc />
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return await _client.Send(request, cancellationToken);
		}
	}
}