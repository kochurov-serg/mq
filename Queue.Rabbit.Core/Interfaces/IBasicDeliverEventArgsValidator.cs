using RabbitMQ.Client.Events;

namespace Queue.Rabbit.Core.Interfaces
{
	/// <summary>
	/// Validate parameters
	/// </summary>
	public interface IBasicDeliverEventArgsValidator
	{
		/// <summary>
		/// Validate arguments
		/// </summary>
		/// <param name="args">args</param>
		void Validate(BasicDeliverEventArgs args);
	}
}