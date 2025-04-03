using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Queue.Server.Abstractions
{
	public class QueueConnectionInfo: ConnectionInfo
	{
		/// <inheritdoc />
		public override Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<X509Certificate2>(null);
        }

		/// <inheritdoc />
		public override X509Certificate2 ClientCertificate { get; set; }

		/// <inheritdoc />
		public override string Id { get; set; } = Guid.NewGuid().ToString("N");

		/// <inheritdoc />
		public override IPAddress LocalIpAddress { get; set; }

		/// <inheritdoc />
		public override int LocalPort { get; set; }

		/// <inheritdoc />
		public override IPAddress RemoteIpAddress { get; set; }

		/// <inheritdoc />
		public override int RemotePort { get; set; }
	}

	/// <summary>
	/// Mqp context
	/// </summary>
	public class QueueContext : HttpContext
	{
		private QueueRequest QueueRequest { get; }

		private QueueResponse QueueResponse { get; }

		/// <inheritdoc />
		public QueueContext(IFeatureCollection features, QueueResponse response, QueueRequest request)
		{
			Features = features;
			QueueResponse = response;
			QueueRequest = request;

			QueueResponse.Initialize(this);
			QueueRequest.Initialize(this);
		}

		/// <inheritdoc />
		public override void Abort()
		{
		}

		/// <inheritdoc />
		public override IFeatureCollection Features { get; }

		/// <inheritdoc />
		public override HttpRequest Request => QueueRequest;

		/// <inheritdoc />
		public override HttpResponse Response => QueueResponse;

		/// <inheritdoc />
		public override ConnectionInfo Connection => new QueueConnectionInfo();

		/// <inheritdoc />
		public override WebSocketManager WebSockets => throw new NotSupportedException("Queue not support web sockets");

		/// <inheritdoc />
		public override ClaimsPrincipal User { get; set; }

		/// <inheritdoc />
		public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

		/// <inheritdoc />
		public override IServiceProvider RequestServices { get; set; }

		/// <inheritdoc />
		public override CancellationToken RequestAborted { get; set; } = CancellationToken.None;

		private string _traceIdentifier;
		/// <inheritdoc />
		public override string TraceIdentifier
		{
			get
			{
				if (_traceIdentifier == null)
				{
					_traceIdentifier = $"{Request.Scheme}:{Guid.NewGuid().ToString("N").Substring(20)}";
				}

				return _traceIdentifier;
			}
			set => _traceIdentifier = value;
		}

		/// <inheritdoc />
		public override ISession Session { get; set; }
	}
}
