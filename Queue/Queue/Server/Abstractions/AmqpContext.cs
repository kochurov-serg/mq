using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using AuthenticationManager = Microsoft.AspNetCore.Http.Authentication.AuthenticationManager;

namespace Notification.Amqp.Server.Abstractions
{
	/// <summary>
	/// Amqp context
	/// </summary>
	public class AmqpContext : HttpContext
	{
		internal AmqpHttpRequest AmqpRequest { get; private set; }
		internal AmqpHttpResponse AmqpResponse { get; private set; }

		/// <inheritdoc />
		public AmqpContext(IFeatureCollection features)
		{
			Features = features;
		}

		/// <inheritdoc />
		public void InitializeResponse(AmqpHttpResponse response)
		{
			if (Response != null)
				throw new ArgumentException("response already initialize");

			AmqpResponse = response;
		}

		/// <inheritdoc />
		public void InitializeRequest(AmqpHttpRequest request)
		{
			if (Request != null)
				throw new ArgumentException("request already initialize");

			AmqpRequest = request;
		}

		/// <inheritdoc />
		public override void Abort()
		{

		}

		/// <inheritdoc />
		public override IFeatureCollection Features { get; }

		/// <inheritdoc />
		public override HttpRequest Request { get => AmqpRequest; }

		/// <inheritdoc />
		public override HttpResponse Response { get => AmqpResponse; }

		/// <inheritdoc />
		public override ConnectionInfo Connection { get; }

		/// <inheritdoc />
		public override WebSocketManager WebSockets { get => throw new NotSupportedException("WebSocket not supported amqp protocol"); }

		/// <inheritdoc />
		public override AuthenticationManager Authentication { get; }

		/// <inheritdoc />
		public override ClaimsPrincipal User { get; set; }

		/// <inheritdoc />
		public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

		/// <inheritdoc />
		public override IServiceProvider RequestServices { get; set; }

		/// <inheritdoc />
		public override CancellationToken RequestAborted { get; set; }

		/// <inheritdoc />
		public override string TraceIdentifier { get; set; }

		/// <inheritdoc />
		public override ISession Session { get; set; }
	}
}
