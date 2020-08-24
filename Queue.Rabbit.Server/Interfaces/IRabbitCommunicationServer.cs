using System;
using System.Threading.Tasks;
using Queue.Rabbit.Core.Repeat;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server.Interfaces
{
	public interface IRabbitCommunicationServer : IDisposable
	{
		Task Init();

		/// <summary>
		/// Создать подключение
		/// </summary>
		/// <param name="received">Подписчик</param>
		void CreateBasicConsumer(EventHandler<BasicDeliverEventArgs> received);

		/// <summary>
		/// Отправить данные
		/// </summary>
		/// <returns></returns>
		Task Send(BasicDeliverEventArgs args);

		IBasicProperties CreateBasicProperties();

		/// <summary>
		/// Отправить сообщение на повторную попытку
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		Task SendException(BasicDeliverEventArgs args);
		/// <summary>
		/// Подтверждение обработки
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		Task Ack(BasicDeliverEventArgs args);

		Task SendError(BasicDeliverEventArgs args);
		/// <summary>
		/// Wait and retry message
		/// </summary>
		/// <param name="args">message</param>
		/// <param name="config">retry queue</param>
		/// <returns></returns>
		Task<bool> Retry(BasicDeliverEventArgs args, RepeatConfig config);
	}
}