using System.IO;
using System.Threading.Tasks;

namespace Queue.Core
{
	public static class StreamExtensions
	{
		/// <summary>
		/// Read all bytes
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
		{
			byte[] body;

			if (stream is MemoryStream memoryStream)
			{
				body = memoryStream.GetBuffer();
			}
			else
			{
				body = new byte[stream.Length];
				await stream.ReadAsync(body, 0, (int)stream.Length);
			}

			return body;
		}
	}
}
