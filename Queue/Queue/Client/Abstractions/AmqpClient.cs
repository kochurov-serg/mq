using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Notification.Amqp.Client.Abstractions.Interfaces;
using Notification.Amqp.Extensions;
using Notification.Amqp.Server.RabbitMq;

namespace Notification.Amqp.Client.Abstractions
{
	/// <summary>
	/// Реализация общего клиента отправки сообщений в по протоколу amqp
	/// </summary>
	public class VirtualAmqpClient : IVirtualAmqpClient
	{
		private readonly IAmqpClient _client;
		private readonly ILogger<VirtualAmqpClient> _log;
		private readonly IHttpRequestPropertiesParser _propertiesParser;
		private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
		private bool _isStarted;

		readonly MemoryCache _callbacks =
			new MemoryCache(
				new MemoryCacheOptions { Clock = new SystemClock(), ExpirationScanFrequency = TimeSpan.FromSeconds(5) });

		public VirtualAmqpClient(IAmqpClient client, ILogger<VirtualAmqpClient> log, IHttpRequestPropertiesParser propertiesParser)
		{
			_client = client;
			_log = log;
			_propertiesParser = propertiesParser;
		}

		private void CheckParams(Uri baseUri, HttpRequestMessage request)
		{
			if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));
			if (request == null) throw new ArgumentNullException(nameof(request));
		}

		public async Task<HttpResponseMessage> Send(Uri baseUri, HttpRequestMessage request)
		{
			CheckParams(baseUri, request);

			var options = AmqpRequestOptions.DefaultOptions;
			options = _propertiesParser.Parse(options, request.Properties);
			var response = await Send(baseUri, request, options);

			return response;
		}

		public async Task<HttpResponseMessage> Send(Uri baseUri, HttpRequestMessage request,
			AmqpRequestOptions options)
		{
			CheckParams(baseUri, request);

			if (options == null)
				options = AmqpRequestOptions.DefaultOptions;

			var response = new AmqpResponse
			{
				RequestOptions = options
			};

			var cancellationToken = new CancellationToken();

			var cancellationTokenSource =
				CancellationTokenSource.CreateLinkedTokenSource(options.CancellationToken, cancellationToken);

			var competionSource = response.Response;

			if (options.CallType == AmqpCallType.Rpc && options.Timeout == 0)
			{
				options.Timeout = AmqpCommunication.RPC_TIMEOUT;
				_log.LogWarning(
					$"Call {AmqpCommunication.ProtocolName} by type {options.CallType} must be expired time. Set default value {options.Timeout}");
			}

			var correlationId = request.Headers.GetCorrelationHeader();

			if (!string.IsNullOrWhiteSpace(correlationId))
			{
				if (options.CallType == AmqpCallType.Call)
				{
					options.Timeout = AmqpCommunication.Call_TIMEOUT;
					_log.LogInformation(
						$"Identifier callback set in headers. But calltype {options.CallType}. This type specified not sent response. Response will cached by {options.Timeout} CorrelationId {correlationId}.");
				}

				_callbacks.Set(correlationId, response, options.Expires);
			}

			try
			{
				await _client.Send(baseUri, request, options);

				if (options.CallType == AmqpCallType.Call)
				{
					_log.LogTrace(
						$"Call type {options.CallType}. Server not sent ansewer. Default http status set 200.");
					response.Response.SetResult(new HttpResponseMessage(HttpStatusCode.OK));
					response.EndTiming(TaskStatus.RanToCompletion);
				}
			}
			catch (Exception e)
			{
				cancellationTokenSource.Cancel();
				_log.LogError(e, "Request send error");
				competionSource.SetException(new Exception($"no sent request to resource {baseUri} {request.RequestUri}",
					e));
				response.EndTiming(TaskStatus.Faulted);
				response.Exception = e;

				return await competionSource.Task;
			}

			await Task.WhenAny(competionSource.Task, Task.Delay(options.Expires, cancellationTokenSource.Token));

			if (!competionSource.Task.IsCompleted)
			{
				response.EndTiming(TaskStatus.Canceled);
				competionSource.SetCanceled();
			}

			return await competionSource.Task;
		}

		public AmqpResponse Extract(string correlationId)
		{
			var response = Get(correlationId);
			_callbacks.Remove(correlationId);

			return response;
		}

		public AmqpResponse Get(string correlationId)
		{
			var response = _callbacks.Get<AmqpResponse>(correlationId);

			return response;
		}

		/// <inheritdoc />
		public async Task StartAsync()
		{
			try
			{
				await semaphore.WaitAsync();

				if (_isStarted)
				{
					return;
				}

				_isStarted = true;
				await _client.Subscribe(response =>
				{
					var correlationId = response.Headers.GetCorrelationHeader();
					_log.LogTrace($"Get callback from cache {correlationId}");
					var amqpResponse = Get(correlationId);
					if (amqpResponse == null)
					{
						var requestOptions = AmqpRequestOptions.DefaultOptions;
						requestOptions.CallType = AmqpCallType.Async;
						requestOptions.Timeout = 30;
						_log.LogTrace($"Callback not found {correlationId}. Create temporary callback. For {requestOptions.CallType.ToString()} type");

						amqpResponse = new AmqpResponse { RequestOptions = requestOptions };
						_callbacks.Set(correlationId, amqpResponse, amqpResponse.RequestOptions.Expires);

						_log.LogInformation($"Temporary save completed. correlationId {correlationId}.");
					}
					else
					{
						if (amqpResponse.Status == TaskStatus.Canceled)
						{
							_log.LogInformation($"Запрос {correlationId} был отменен. Удаление из очереди ожидания ответов");
							_callbacks.Remove(correlationId);

							return true;
						}
					}

					if (amqpResponse.RequestOptions.CallType == AmqpCallType.Rpc)
					{
						_log.LogTrace($"Parsing {correlationId}. {amqpResponse.RequestOptions.CallType} type. Remove callback");
						_callbacks.Remove(correlationId);
					}

					amqpResponse.Response.SetResult(response);
					amqpResponse.EndTiming(TaskStatus.RanToCompletion);

					return true;
				});
			}
			finally
			{
				semaphore.Release(1);
			}
		}

		/// <inheritdoc />
		public Task StopAsync()
		{
			_isStarted = false;
			return Task.CompletedTask;
		}
	}
}