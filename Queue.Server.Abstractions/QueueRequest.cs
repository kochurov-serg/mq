using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Queue.Server.Abstractions
{
	/// <summary>
	/// Запрос по протоколу Mqp
	/// </summary>
	public class QueueRequest : HttpRequest
	{
		private QueueContext _context;

		public void Initialize(QueueContext context)
		{
			if (HttpContext != null)
				throw new ArgumentException("Context already initialize");

			_context = context;
		}

		/// <inheritdoc />
		public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc />
		public override HttpContext HttpContext => _context;

		/// <inheritdoc />
		public override string Method { get; set; }

		/// <inheritdoc />
		public override string Scheme { get; set; } = "noSet";

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
		public override IRequestCookieCollection Cookies { get; set; }

		/// <inheritdoc />
		public override long? ContentLength { get; set; }

		/// <inheritdoc />
		public override string ContentType { get; set; }

		/// <inheritdoc />
		public override Stream Body { get; set; }

		/// <inheritdoc />
		public override bool HasFormContentType { get; } = false;

		/// <inheritdoc />
		public override IFormCollection Form { get; set; }

		public override PipeReader BodyReader => PipeReader.Create(Body, new StreamPipeReaderOptions());
	}
}