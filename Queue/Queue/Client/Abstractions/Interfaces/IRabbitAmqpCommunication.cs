using System;
using System.Threading.Tasks;
using Notification.Client.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Amqp.Server.RabbitMq
{
	/// <inheritdoc />
	public interface IRabbitAmqpCommunication : IDisposable
	{
		RabbitDeclare Declare(string exchangeName, string queueName, bool autoDelete);
		/// <summary>
		/// Создание Exchange Для параллельной обработки нескольких запросов. Либо возврат ответа на несколько сервисов
		/// </summary>
		/// <param name="source">Exchange На который будут приходить параллельные запросы (бизнес логика обработки нескольких запросов)</param>
		/// <param name="queue">Очередь на которые будут перенаправляться эти запросы</param>
		/// <param name="destinations">Exchange назначения</param>
		void DeclareParallel(string source, string queue, params string[] destinations);
		/// <summary>
		/// Создать подключение
		/// </summary>
		/// <param name="queue">Очередь сообщений</param>
		/// <param name="received">Событие обработки</param>
		/// <param name="register">Событие регистрации</param>
		void CreateBasicConsumer(string queue, EventHandler<BasicDeliverEventArgs> received, EventHandler<ConsumerEventArgs> register);

		/// <summary>
		/// Отправить сообщение
		/// </summary>
		/// <param name="exchangeName">Наименование exchange</param>
		/// <param name="queueName">Очередь</param>
		/// <param name="args">Параметры отправки</param>
		/// <returns></returns>
		Task Send(string exchangeName, string queueName, BasicDeliverEventArgs args);
		/// <summary>
		/// Отклонить сообщение из основной очереди и направить его в очередь ошибок.
		/// </summary>
		/// <param name="args">Параметры сообщения</param>
		/// <param name="countRetry">Количество попыток повторной обработки</param>
		/// <param name="exchangeName">Наименование Exchange</param>
		/// <param name="queueName"></param>
		/// <returns></returns>
		Task SendError(BasicDeliverEventArgs args, int countRetry, string queueName);
		/// <summary>
		/// Инициализация подключения
		/// </summary>
		/// <returns></returns>
		Task Init();
		/// <summary>
		/// Канал обмена данными
		/// </summary>
		IModel Channel { get; }
	}
}