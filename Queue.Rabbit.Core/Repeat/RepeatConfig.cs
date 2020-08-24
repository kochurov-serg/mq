using System;

namespace Queue.Rabbit.Core.Repeat
{
	public class RepeatConfig
	{
		/// <summary>
		/// Header name count repeat request on exception or 5xx status code (0 - no repeat, -1 - forever, or any int number)
		/// </summary>
		public const string RepeatCount = "repeat-count";
		/// <summary>
		/// Delay in milliseconds before repeat
		/// </summary>
		public const string RepeatDelay = "repeat-delay";
		/// <summary>
		/// Strategy delay (const, progression)
		/// </summary>
		public const string StrategyRepeatDelay = "repeat-delay-strategy";

		public int Count { get; set; }

		public RepeatStrategy Strategy { get; set; }

		public TimeSpan Delay { get; set; } = TimeSpan.Zero;
	}
}