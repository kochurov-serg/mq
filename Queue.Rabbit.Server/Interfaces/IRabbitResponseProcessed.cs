using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Server.Interfaces
{
	/// <summary>
	/// Response processed
	/// </summary>
	public interface IRabbitResponseProcessed
	{
		/// <summary>
		/// Manipulation status, 500 send to retry, 400 send to error
		/// </summary>
		/// <param name="requestArgs">Request</param>
		/// <param name="context"></param>
		/// <returns></returns>
		Task Handle(BasicDeliverEventArgs requestArgs, HttpContext context);
	}
}