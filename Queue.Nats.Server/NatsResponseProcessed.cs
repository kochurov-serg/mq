using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using NATS.Client;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Queue.Nats.Server
{
	/// <summary>
	/// send response
	/// </summary>
	public class NatsResponseProcessed : INatsResponseProcessed
	{
		private readonly ILogger<NatsResponseProcessed> _log;

		public NatsResponseProcessed(ILogger<NatsResponseProcessed> log)
		{
			_log = log;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <param name="context"></param>
		/// <param name="connection"></param>
		/// <returns></returns>
		public async Task ProcessResponse(MsgHandlerEventArgs args, HttpContext context, IConnection connection)
		{
			if (args.Message.Reply != null)
			{
				_log.LogTrace($"Create response to reply {args.Message.Reply}");

				if (context.Response.Body.CanSeek)
					context.Response.Body.Position = 0;

				var response = new HttpResponseMessage
				{
					StatusCode = (HttpStatusCode)context.Response.StatusCode,
					Content = new StreamContent(context.Response.Body),
				};

				response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Response.ContentType);
				response.Content.Headers.ContentLength = context.Response.Body.Length;

				foreach (var header in context.Response.Headers)
				{
					response.Headers.TryAddWithoutValidation(header.Key, header.Value.AsEnumerable());
				}

				var content = new HttpMessageContent(response);
				var bytes = await content.ReadAsByteArrayAsync();
				_log.LogTrace($"Send response to reply {args.Message.Reply}");
				connection.Publish(args.Message.Reply, bytes);
				_log.LogTrace($"Response to reply {args.Message.Reply} sended");
			}
			else
			{
				_log.LogTrace($"reply is empty, response not be sended. request url {context.Request.GetDisplayUrl()}");
			}
		}
	}
}