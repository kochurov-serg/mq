using System;
using System.IO;
using System.Threading.Tasks;

namespace Notification.Amqp.Extensions
{
	internal static class StreamExtensions
	{
		internal static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
		{
			byte[] body;
			var memoryStream = stream as MemoryStream;

			if (stream != null)
			{
				body = memoryStream.GetBuffer();
			}
			else
			{
				var buffer = new Memory<byte>();
				await stream.ReadAsync(buffer);
				body = buffer.ToArray();
			}

			return body;
		}
	}
}
