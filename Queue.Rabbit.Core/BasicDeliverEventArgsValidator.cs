using System;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using Queue.Rabbit.Core.Interfaces;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Core
{
	/// <inheritdoc />
	public class BasicDeliverEventArgsValidator : IBasicDeliverEventArgsValidator
	{
		/// <inheritdoc />
		public void Validate(BasicDeliverEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException(nameof(args));

			var props = args.BasicProperties;

			if (args.BasicProperties == null)
				throw new ArgumentNullException(nameof(args.BasicProperties));

			var contentType = props.Headers.GetOrDefaultString(QueueHeaders.Uri);
			if (string.IsNullOrWhiteSpace(contentType))
				throw new ArgumentNullException(QueueHeaders.Uri, $"Headers {QueueHeaders.Uri} value cannot be null or empty.");
		}
	}
}
