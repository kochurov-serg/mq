using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Queue.Rabbit.Core.Repeat;

namespace Queue.Rabbit.Server.Repeat;

public static class RepeatExtensions
{
    public static RepeatConfig ParseRepeat(this IDictionary<string, StringValues> headers)
    {
        if (headers == null)
            throw new ArgumentNullException(nameof(headers));

        if (!headers.TryGetValue(RepeatConfig.RepeatCount, out var repeatCountHeader))
            return null;

        headers.TryGetValue(RepeatConfig.RepeatDelay, out var repeatDelayHeader);
        headers.TryGetValue(RepeatConfig.StrategyRepeatDelay, out var strategyHeader);


        if (!int.TryParse(repeatCountHeader.ToString(), out var repeatCount))
        {
            repeatCount = 1;
        }

        TimeSpan.TryParse(repeatDelayHeader.ToString(), out var delay);
        if (!Enum.TryParse<RepeatStrategy>(strategyHeader.ToString(), true, out var strategy))
        {
            strategy = RepeatStrategy.Progression;
        }

        var config = new RepeatConfig
        {
            Count = repeatCount,
            Delay = delay,
            Strategy = strategy
        };

        return config;
    }
}