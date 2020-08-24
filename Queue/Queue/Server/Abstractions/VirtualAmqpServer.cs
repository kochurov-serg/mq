using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Notification.Amqp.Server.Abstractions.Interfaces;

namespace Notification.Amqp.Server.Abstractions
{
	/// <inheritdoc />
	public class VirtualAmqpServer : IVirtualAmqpServer
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IAmqpServer _server;

		/// <inheritdoc />
		public IFeatureCollection Features { get; }

		/// <inheritdoc />
		public VirtualAmqpServer(IServiceScopeFactory scopeFactory, IAmqpServer server)
		{
			Features = new FeatureCollection();
			
			_scopeFactory = scopeFactory;
			_server = server;
		}

		/// <inheritdoc />
		public async Task StartAsync(IApplicationBuilder applicationBuilder, CancellationToken cancellationToken)
		{
			var process = applicationBuilder.Build();

			async Task Delegate(HttpContext context)
			{
				if (context == null) throw new ArgumentNullException(nameof(context));

				using (var scope = _scopeFactory.CreateScope())
				{
					var accessor =scope.ServiceProvider.GetService<IHttpContextAccessor>();
					accessor.HttpContext = context;
					context.RequestServices = scope.ServiceProvider;

					await process(context).ConfigureAwait(false);
				}
			}

			await _server.StartAsync(Delegate, cancellationToken);
		}

		/// <inheritdoc />
		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _server.StopAsync(cancellationToken);
		}
	}
}
