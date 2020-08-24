using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Queue.Kafka.Server
{
	public class KafkaServerOption
	{
		public List<Uri> Server { get; set; } = new List<Uri>();

		public bool ForceResponseStream { get; set; }

		public ConsumerConfig ConsumerConfig { get; set; }

		public ProducerConfig ProducerConfig { get; set; }
	}
}