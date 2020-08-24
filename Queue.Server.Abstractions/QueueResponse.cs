using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Queue.Server.Abstractions
{
	/// <summary>
	/// Mqp response
	/// </summary>
	public class QueueResponse : HttpResponse
	{
		public static int PermanentRedirectCode = 301;
		public static int RedirectCode = 302;
		private QueueContext _context;

		public void Initialize(QueueContext context)
		{
			if (HttpContext != null)
				throw new ArgumentException("Context already initialize");

			_context = context;
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
			StatusCode = permanent ? PermanentRedirectCode : RedirectCode;
		}

		/// <inheritdoc />
		public override HttpContext HttpContext => _context;

		/// <inheritdoc />
		public override int StatusCode { get; set; } = StatusCodes.Status200OK;

		/// <inheritdoc />
		public override IHeaderDictionary Headers { get; } = new HeaderDictionary();

		/// <inheritdoc />
		public override Stream Body { get; set; }

		/// <inheritdoc />
		public override long? ContentLength { get; set; } = 0;

		/// <inheritdoc />
		public override string ContentType { get; set; } = "application/json";

		/// <inheritdoc />
		public override IResponseCookies Cookies { get; } = null;

		/// <inheritdoc />
		public override bool HasStarted { get; } = false;
	}
}