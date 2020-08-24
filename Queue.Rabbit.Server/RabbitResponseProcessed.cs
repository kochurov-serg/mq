using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Queue.Core;
using Queue.Rabbit.Server.Extensions;
using Queue.Rabbit.Server.Interfaces;
using Queue.Rabbit.Server.Repeat;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace Queue.Rabbit.Server
{
	/// <inheritdoc />
	public class RabbitResponseProcessed : IRabbitResponseProcessed
	{
		private const long _errorExpiration = 2L * 60 * 60 * 1000;
		private readonly ILogger<RabbitResponseProcessed> _log;
		private readonly IRabbitCommunicationServer _communication;

		public RabbitResponseProcessed(ILogger<RabbitResponseProcessed> log, IRabbitCommunicationServer communication)
		{
			_log = log;
			_communication = communication;
		}

		public async Task Handle(BasicDeliverEventArgs requestArgs, HttpContext context)
		{
			var request = context.Request;
			var response = context.Response;

			var requestProps = requestArgs.BasicProperties;

			_log.LogInformation("{route} response status {status}", requestArgs.RoutingKey, response.StatusCode);

			if (response.StatusCode >= 500)
			{
				_log.LogInformation("Response Status {status}. Retry", response.StatusCode);

				var config = response.Headers.ParseRepeat() ?? request.Headers.ParseRepeat();

				if (config != null)
				{
					if (await _communication.Retry(requestArgs, config))
					return;
				}
			}

			var basicProperties = _communication.CreateBasicProperties()
				.CreateBasicPropertiesResponse(requestArgs.BasicProperties, context.Request, response);

			if (requestProps.ReplyTo == null)
			{
				if (response.StatusCode >= 400)
				{
					var bodyTask = new StreamReader(response.Body).ReadToEndAsync();

					var errorArgs = new BasicDeliverEventArgs
					{
						Body = requestArgs.Body,
						BasicProperties = basicProperties
					};
					errorArgs.BasicProperties.Expiration = _errorExpiration.ToString();
					errorArgs.BasicProperties.Headers.Add("responseUid", context.TraceIdentifier);
					_log.LogTrace("Response {uid}:{response}", context.TraceIdentifier, await bodyTask);
					await _communication.SendError(errorArgs);
				}

				await _communication.Ack(requestArgs);
				return;
			}

			var address = requestProps.ReplyToAddress;

			var args = new BasicDeliverEventArgs
			{
				RoutingKey = address.RoutingKey,
				Body = await response.Body.ReadAllBytesAsync(),
				BasicProperties = basicProperties,
				Exchange = address.ExchangeName
			};

			_log.LogTrace("Response. exchange: {exchange} route: {queue}", address.ExchangeName, args.RoutingKey);

			try
			{
				await _communication.Send(args)
					.ContinueWith(task =>
					{
						if (task.Exception != null)
						{
							_log.LogError(task.Exception, $"Error send message exchange: {address.ExchangeName}, routing {args.RoutingKey}");

							_communication.SendError(args);
						}
						else
						{
							_communication.Ack(requestArgs);
						}
					});
			}
			catch (Exception e)
			{
				_log.LogError(e, "Error send response message");
			}
			_log.LogTrace("Response. exchange: {exchange} route: {queue} sended", address.ExchangeName, args.RoutingKey);
		}
	}
}