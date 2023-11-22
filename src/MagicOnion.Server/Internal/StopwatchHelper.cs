using System.Diagnostics;

namespace MagicOnion.Server.Internal;

internal static class StopwatchHelper
{
#if NET7_0_OR_GREATER
    public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
        => Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);
#else
#pragma warning disable IDE1006 // Naming Styles
    const long TicksPerSecond = TicksPerMillisecond * 1000;
    const long TicksPerMillisecond = 10000;
    static readonly double tickFrequency = (double)TicksPerSecond / Stopwatch.Frequency;
#pragma warning restore IDE1006 // Naming Styles

    public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
        => new TimeSpan((long)((endingTimestamp - startingTimestamp) * tickFrequency));
#endif
}
