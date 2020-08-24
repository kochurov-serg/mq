using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Queue.Rabbit.Client.Interfaces;

namespace Queue.Rabbit.Client
{
	public class RabbitDelegatingHandler : DelegatingHandler
	{
		private readonly IRabbitQueueClient _client;

		public RabbitDelegatingHandler(IRabbitQueueClient client)
		{
			_client = client;
		}

		/// <inheritdoc />
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			await _client.Send(request, cancellationToken);

			return new HttpResponseMessage(HttpStatusCode.OK);
		}
	}
}
