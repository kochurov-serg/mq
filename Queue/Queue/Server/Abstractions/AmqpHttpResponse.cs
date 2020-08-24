using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Notification.Amqp.Server.Abstractions
{
	/// <summary>
	/// Amqp response
	/// </summary>
	public class AmqpHttpResponse : HttpResponse
	{
		private AmqpContext _context;

		public void Initialize(AmqpContext context)
		{
			if (HttpContext != null)
				throw new ArgumentException("Context already initialize");

			_context = context;
			_context.InitializeResponse(this);
		}

		/// <inheritdoc />
		public override void OnStarting(Func<object, Task> callback, object state)
		{

		}

		/// <inheritdoc />
		public override void OnCompleted(Func<object, Task> callback, object state)
		{

		}

		/// <inheritdoc />
		public override void Redirect(string location, bool permanent)
		{

		}

		/// <inheritdoc />
		public override HttpContext HttpContext { get; }

		/// <inheritdoc />
		public override int StatusCode { get; set; } = StatusCodes.Status200OK;

		/// <inheritdoc />
		public override IHeaderDictionary Headers { get; } = new HeaderDictionary();

		/// <inheritdoc />
		public override Stream Body { get; set; } = new MemoryStream();

		/// <inheritdoc />
		public override long? ContentLength { get; set; } = 0;

		/// <inheritdoc />
		public override string ContentType { get; set; } = "application/json";

		/// <inheritdoc />
		public override IResponseCookies Cookies { get; }

		/// <inheritdoc />
		public override bool HasStarted { get; }
	}
}