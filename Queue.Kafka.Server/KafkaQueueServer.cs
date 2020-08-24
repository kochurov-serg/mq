using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Queue.Kafka.Server
{
	public class KafkaQueueServer : IKafkaQueueServer, IDisposable
	{
		private readonly ILogger<KafkaQueueServer> _log;
		private readonly KafkaServerOption _options;
		private readonly IKafkaConverter _converter;
		private readonly IKafkaResponseProcessed _responseProcessed;
		private readonly CancellationTokenSource _token = new CancellationTokenSource();

		public KafkaQueueServer(ILogger<KafkaQueueServer> log, KafkaServerOption options, IKafkaConverter converter, IKafkaResponseProcessed responseProcessed)
		{
			_log = log;
			_options = options;
			_converter = converter;
			_responseProcessed = responseProcessed;
		}

		/// <inheritdoc />
		public async Task Start(RequestDelegate requestDelegate, CancellationToken cancellationToken)
		{
			var hosts = _options.Server.Select(x => x.Host).ToList();
			if (hosts == null)
				throw new ArgumentException($"{nameof(_options.Server)} must be set at least one");

			await Task.Run(async () =>
			{
				var token = CancellationTokenSource.CreateLinkedTokenSource(_token.Token, cancellationToken);

				using (var consumer = new ConsumerBuilder<byte[], byte[]>(_options.ConsumerConfig).Build())
				{
					consumer.Subscribe(_options.Server.Select(x => x.Host));

					try
					{
						while (true)
						{
							var message = consumer.Consume(token.Token);

							try
							{
								var context = await _converter.Parse(message, new FeatureCollection());

								if (context != null)
								{
									await requestDelegate(context);
									await _responseProcessed.Handle(message, context);
								}
								else
								{
									_log.LogError("Cannot parse context");
								}
							}
							catch (Exception e)
							{
								_log.LogError(e,
									$"Unprocessed exception,topic: {message.Topic}, partition: {message.Partition.Value} offset: {message.Offset.Value}");
							}
						}
					}
					catch (OperationCanceledException e)
					{
						_log.LogInformation(e,"Kafka server cancelled. Close consumer");
						consumer.Close();
					}
				}
			}, cancellationToken);
		}

		/// <inheritdoc />
		public Task Stop(CancellationToken cancellationToken)
		{
			_token.Cancel();
			Dispose();
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}
	}
}
