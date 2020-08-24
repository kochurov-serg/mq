using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Notification.Amqp.Server.Abstractions
{
	/// <summary>
	/// Запрос по протоколу Amqp
	/// </summary>
	public class AmqpHttpRequest : HttpRequest
	{
		private AmqpContext _context;

		public AmqpHttpRequest()
		{
		}

		public void Initialize(AmqpContext context)
		{
			if (HttpContext != null)
				throw new ArgumentException("Context already initialize");

			_context = context;
			_context.InitializeRequest(this);
		}

		/// <inheritdoc />
		public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override HttpContext HttpContext
		{
			get => _context;
		}

		/// <inheritdoc />
		public override string Method { get; set; }

		/// <inheritdoc />
		public override string Scheme { get; set; } = "amqp";

		/// <inheritdoc />
		public override bool IsHttps { get; set; } = false;

		/// <inheritdoc />
		public override HostString Host { get; set; }

		/// <inheritdoc />
		public override PathString PathBase { get; set; }

		/// <inheritdoc />
		public override PathString Path { get; set; }

		/// <inheritdoc />
		public override QueryString QueryString { get; set; }

		/// <inheritdoc />
		public override IQueryCollection Query { get; set; }

		/// <inheritdoc />
		public override string Protocol { get; set; }

		/// <inheritdoc />
		public override IHeaderDictionary Headers { get; } = new HeaderDictionary();

		/// <inheritdoc />
		public override IRequestCookieCollection Cookies { get; set; } = new RequestCookieCollection();

		/// <inheritdoc />
		public override long? ContentLength { get; set; }

		/// <inheritdoc />
		public override string ContentType { get; set; }

		/// <inheritdoc />
		public override Stream Body { get; set; }

		/// <inheritdoc />
		public override bool HasFormContentType { get; }

		/// <inheritdoc />
		public override IFormCollection Form { get; set; }

	}
}