using System;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Options;

namespace Queue.Rabbit.Server
{
	public class RabbitServerOptions
	{
		public bool RetryForever { get; set; } = false;

		public RabbitConnection Connection { get; set; }

		public QosOptions Qos { get; set; } = new QosOptions();

		public AppQueueOption Queue { get; set; } = new AppQueueOption();
		/// <summary>
		/// Delay queue, if null then message be lost if return exception or http status more or equal 500
		/// </summary>
		public DelayQueueOption DefaultDelayQueue { get; set; } = new DelayQueueOption();
		/// <summary>
		/// Delay politics. Allow different 
		/// </summary>
		public DelayOptions DelayOptions { get; set; } = new DelayOptions();
		/// <summary>
		/// Error queue, if null then error is not be set queue
		/// </summary>
		public ErrorQueueOption ErrorQueue { get; set; } = new ErrorQueueOption();
		/// <summary>
		/// Force create response stream
		/// </summary>
		public bool ForceResponseStream { get; set; } = false;
	}
}