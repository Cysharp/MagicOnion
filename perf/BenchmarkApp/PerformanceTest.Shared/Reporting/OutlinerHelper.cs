namespace PerformanceTest.Shared.Reporting;

public static partial class OutlinerHelper
{
    public static IReadOnlyList<double> RemoveOutlinerByIQR(IReadOnlyList<double> data, double upperLimit)
    {
        if (data.Count == 0)
            return data;

        var sorted = data.ToArray();
        Array.Sort(sorted);
        var sortedSpan = sorted.AsSpan();

        var lowerBound = GetLowerBound(sortedSpan);
        var upperBound = GetUpperBound(sortedSpan);

        var result = new List<double>(data.Count);
        foreach (var value in data)
        {
            if (value >= lowerBound && value <= upperBound && value <= upperLimit)
            {
                result.Add(value);
            }
        }

        return result;
    }

    internal static double GetLowerBound(ReadOnlySpan<double> sortedData)
    {
        var q1 = GetPercentile(sortedData, 25);
        var q3 = GetPercentile(sortedData, 75);
        var iqr = q3 - q1;
        return q1 - 1.5 * iqr;
    }

    internal static double GetUpperBound(ReadOnlySpan<double> sortedData)
    {
        var q1 = GetPercentile(sortedData, 25);
        var q3 = GetPercentile(sortedData, 75);
        var iqr = q3 - q1;
        return q3 + 1.5 * iqr;
    }

    static double GetPercentile(ReadOnlySpan<double> sortedData, double percentile)
    {
        var n = sortedData.Length;
        var position = (n - 1) * percentile / 100.0 + 1;
        if (position == 1) return sortedData[0];
        if (position == n) return sortedData[n - 1];
        var k = (int)Math.Floor(position) - 1;
        var d = position - Math.Floor(position);
        return sortedData[k] + d * (sortedData[k + 1] - sortedData[k]);
    }
}
