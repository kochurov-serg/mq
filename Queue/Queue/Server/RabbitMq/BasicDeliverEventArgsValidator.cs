using System;
using Microsoft.Net.Http.Headers;
using RabbitMQ.Client.Events;

namespace Notification.Amqp.Server.RabbitMq
{
	public interface IBasicDeliverEventArgsValidator
	{
		void Validate(BasicDeliverEventArgs args);
	}

	public class BasicDeliverEventArgsValidator : IBasicDeliverEventArgsValidator
	{

		public void Validate(BasicDeliverEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException(nameof(args));

			var props = args.BasicProperties;

			if (args.BasicProperties == null)
				throw new ArgumentNullException(nameof(args.BasicProperties));

			var contentType = props.Headers.GetOrDefaultString(HeaderNames.ContentType);
			if (string.IsNullOrWhiteSpace(contentType))
				throw new ArgumentNullException(nameof(props.Headers), "value cannot be null or empty. Value must be start with slash");
		}
	}
}
