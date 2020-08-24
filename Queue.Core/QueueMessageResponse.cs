using System;
using System.Collections.Generic;
using System.IO;

namespace Queue.Core
{
	/// <summary>
	/// message (request/response)
	/// </summary>
	public class QueueMessageResponse
	{
		/// <summary>
		/// Http status response
		/// </summary>
		public int Status { get; set; }
		/// <summary>
		/// Body
		/// </summary>
		public Stream Body { get; set; }
		/// <summary>
		/// Headers
		/// </summary>
		public IDictionary<string, IEnumerable<string>> Headers { get; set; }
	}
}
