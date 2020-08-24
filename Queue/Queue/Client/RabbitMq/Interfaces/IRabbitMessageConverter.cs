﻿using System.Net.Http;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Notification.Amqp.Client.RabbitMq
{
	/// <summary>
	/// Ковертация сообщения в ответ запроса
	/// </summary>
	public interface IRabbitMessageConverter
	{
		/// <summary>
		/// Попытаться распознать ответ
		/// </summary>
		/// <param name="args">Ответ от RabbitMq</param>
		/// <returns></returns>
		Task<HttpResponseMessage> TryParse(BasicDeliverEventArgs args);
	}
}