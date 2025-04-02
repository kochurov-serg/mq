using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace Queue.Rabbit.Core.Options;

/// <summary>
/// Сообщение попало в отложенную обработку
/// </summary>
public class DelayQueueOption : QueueOption
{
    public DelayQueueOption()
    {
        Uri = new Uri("delay", UriKind.Relative);
        ExpiresQueue = TimeSpan.Zero;
        Ttl = TimeSpan.FromSeconds(10);
    }

    public DelayQueueOption(string uri, TimeSpan ttl)
    {
        Uri = new Uri(uri, UriKind.Relative);
        ExpiresQueue = TimeSpan.Zero;
        Ttl = ttl;
    }

    /// <inheritdoc />
    public override string ToString() => $"queue {Uri} Time to live message {Ttl}";
}

public class DelayOptions
{
    public const string Delay = "delay";
    public const char Separator = '-';
    public const string Day = "day";
    public const string Hour = "hour";
    public const string Min = "min";
    public const string Sec = "sec";

    public List<DelayQueueOption> QueueOptions { get; set; }

    /// <summary>
    /// Create interval 30 sec,1min, 2min,5min,10min,15min,30min,1hour,4hour,12hour,1day
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<DelayQueueOption> CreateInterval()
    {
        yield return CreateDelayOption(TimeSpan.FromSeconds(30));
        yield return CreateDelayOption(TimeSpan.FromMinutes(1));
        yield return CreateDelayOption(TimeSpan.FromMinutes(2));
        yield return CreateDelayOption(TimeSpan.FromMinutes(5));
        yield return CreateDelayOption(TimeSpan.FromMinutes(10));
        yield return CreateDelayOption(TimeSpan.FromMinutes(15));
        yield return CreateDelayOption(TimeSpan.FromMinutes(30));
        yield return CreateDelayOption(TimeSpan.FromHours(1));
        yield return CreateDelayOption(TimeSpan.FromHours(4));
        yield return CreateDelayOption(TimeSpan.FromHours(12));
        yield return CreateDelayOption(TimeSpan.FromHours(24));
    }

    private static StringBuilder AppendTimePart(StringBuilder builder, int value, string name)
    {
        if (value == 0)
            return builder;

        builder
            .Append(value)
            .Append(name)
            .Append(Separator);
        return builder;
    }

    public static DelayQueueOption CreateDelayOption(TimeSpan timeSpan)
    {
        var days = timeSpan.Days;
        var hours = timeSpan.Hours;
        var min = timeSpan.Minutes;
        var sec = timeSpan.Seconds;
        var builder = new StringBuilder(Delay)
            .Append(Separator);
        AppendTimePart(builder, days, Day);
        AppendTimePart(builder, hours, Hour);
        AppendTimePart(builder, min, Min);
        AppendTimePart(builder, sec, Sec);

        var name = builder.ToString(0, builder.Length - 1);

        return new DelayQueueOption(name, timeSpan);
    }
}

public class RabbitRequestOption
{
    public static string RequestProperty = "rabbit-request";

    public TimeSpan Delay { get; set; } = TimeSpan.Zero;

    public ConnectionFactory ConnectionFactory { get; set; }
}