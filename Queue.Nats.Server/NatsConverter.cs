using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NATS.Client;
using Queue.Nats.Server.Interfaces;
using Queue.Server.Abstractions.Interfaces;

namespace Queue.Nats.Server
{
	public class NatsConverter : INatsConverter
	{
		private readonly IHttpContextCreator _converter;
		private readonly NatsServerOption _option;

		public NatsConverter(IHttpContextCreator converter, NatsServerOption option)
		{
			_converter = converter;
			_option = option;
		}

		public async Task<HttpContext> Parse(MsgHandlerEventArgs args, IFeatureCollection features)
		{
			var context = await _converter.Create(args.Message.Data, features, !_option.ForceResponseStream && args.Message.Reply == null);

			return context;
		}
	}
}