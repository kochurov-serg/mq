using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
	public class PersonalClient
	{
		private readonly HttpClient _client;

		public PersonalClient(HttpClient client)
		{
			_client = client;
		}

		public async Task<HttpResponseMessage> Send(HttpRequestMessage request,CancellationToken token) => await _client.SendAsync(request, token);
	}
}
