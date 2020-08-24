using System;

namespace Queue.Rabbit.Core.Options
{
	public class QueueOption
	{
		/// <summary>
		/// Equalent host name
		/// </summary>
		public Uri Uri { get; set; }

		public long Expires => (long) ExpiresQueue.TotalMilliseconds;
		/// <summary>
		/// Expires queue
		/// </summary>
		public TimeSpan ExpiresQueue { get; set; }  = TimeSpan.FromDays(30);
		/// <summary>
		/// Message time to live
		/// </summary>
		public long MessageTtl => (long) Ttl.TotalMilliseconds;
		/// <summary>
		///  Message time to live
		/// </summary>
		public TimeSpan Ttl { get; set; }
	}
}