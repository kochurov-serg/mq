using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Queue.Kafka.Client.Interfaces;

namespace Queue.Kafka.Client
{
	public class KafkaMessageHandler : HttpMessageHandler
	{
		private readonly IKafkaQueueClient _client;

		public KafkaMessageHandler(IKafkaQueueClient client)
		{
			_client = client;
		}

		/// <inheritdoc />
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return _client.Send(request, cancellationToken);
		}
	}
}