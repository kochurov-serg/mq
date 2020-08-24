using System;
using System.Collections.Generic;
using System.Text;

namespace Queue.Kafka.Core
{
	public static class KafkaExtensions
	{
		public const string HeaderDelimiter = ";";

		public static byte[] HeaderValue(IEnumerable<string> value) => HeaderValue(string.Join(HeaderDelimiter, value));

		public static byte[] HeaderValue(string value) => Encoding.UTF8.GetBytes(value);
	}
}
