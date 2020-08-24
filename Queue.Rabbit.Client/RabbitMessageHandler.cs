using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Queue.Rabbit.Client.Interfaces;
using Queue.Rabbit.Core.Options;
using RabbitMQ.Client;

namespace Queue.Rabbit.Client
{
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

				var requestOption = (request.Properties.TryGetValue(RabbitRequestOption.RequestProperty, out var requestObjectOption) ?
										requestObjectOption as RabbitRequestOption : null) ??
									new RabbitRequestOption();

				requestOption.ConnectionFactory = Factory;
			}


			var response = await _client.Send(request, cancellationToken);

			return response;
		}
	}
}