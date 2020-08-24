using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Notification.Amqp.Server.Abstractions;

namespace Notification.Amqp.Server.RabbitMq
{
	public interface IAmqpConverter<T>
	{
		Task<AmqpContext> Parse(T args, IFeatureCollection features);
	}
}