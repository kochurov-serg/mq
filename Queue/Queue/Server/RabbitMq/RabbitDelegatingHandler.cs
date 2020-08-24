using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Notification.Amqp.Client.Abstractions.Interfaces;

namespace Notification.Amqp.Server.RabbitMq
{
	public class RabbitDelegatingHandler : DelegatingHandler
	{
		private readonly IVirtualAmqpClient _client;
		private Uri BaseUri { get; set; }

		public RabbitDelegatingHandler(IVirtualAmqpClient client)
		{
			_client = client;
		}

		/// <inheritdoc />
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (BaseUri == null)
				throw new ArgumentNullException($"{nameof(BaseUri)} не установлен для запросов к Rabbit");

			var response = await _client.Send(BaseUri, request, null);

			return response;
		}
	}
}
