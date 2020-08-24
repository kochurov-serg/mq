namespace Queue.Rabbit.Server
{
	public class QosOptions
	{
		public uint PrefetchSize { get; set; }

		public ushort PrefetchCount { get; set; } = 2;

		public bool Global { get; set; } = false;

		/// <inheritdoc />
		public override string ToString() =>
			$"{nameof(PrefetchSize)} {PrefetchSize}, {nameof(PrefetchCount)} {PrefetchCount}, {nameof(Global)} {Global}";


	}
}