using System;
using Queue.Rabbit.Core;

namespace Queue.Rabbit.Client
{
	public class RabbitClientOptions
	{
		/// <summary>
		/// Client name. 
		/// </summary>
		/// <remarks>Ferm application need client name.</remarks>
		public string ClientName { get; set; } = string.Empty;

		public string ClientUnique { get; set; } = string.Empty;

		public bool IsConfiguredClient =>
			!string.IsNullOrWhiteSpace(ClientName) && !string.IsNullOrWhiteSpace(ClientUnique);

		public TimeSpan ResponseWait { get; set; } = TimeSpan.FromMinutes(10);
		public RabbitConnection Connection { get; set; }

		public string ClientQueue => string.Concat(ClientName, "-", ClientUnique);
	}
}