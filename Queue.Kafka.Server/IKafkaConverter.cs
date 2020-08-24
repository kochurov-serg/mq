using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Queue.Kafka.Server
{
	/// <summary>
	/// Kafka request convert to http context
	/// </summary>
	public interface IKafkaConverter
	{
		Task<HttpContext> Parse(ConsumeResult<byte[], byte[]> request, FeatureCollection features);
	}
}