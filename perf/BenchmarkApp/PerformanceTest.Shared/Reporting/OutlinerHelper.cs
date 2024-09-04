namespace PerformanceTest.Shared.Reporting;

public static partial class OutlinerHelper
{
    public static IReadOnlyList<double> RemoveOutlinerByIQR(IReadOnlyList<double> data, double upperLimit)
    {
        if (data.Count == 0)
            return data;

        // sort rps, latency.mean
        var sirted = data.OrderBy(x => x).ToArray();

        // get outliner for rps
        var lowerBound = GetLowerBound(sirted);
        var upperBound = GetUpperBound(sirted);

        // compute tuple in range
        var filteredData = data
            .Where(x => x >= lowerBound && x <= upperBound)
            .Where(x => x <= upperLimit)
            .ToArray();

        return filteredData;
    }

    internal static double GetLowerBound(IReadOnlyList<double> sortedData)
    {
        var Q1 = GetPercentile(sortedData, 25);
        var Q3 = GetPercentile(sortedData, 75);
        var IQR = Q3 - Q1;
        return Q1 - 1.5 * IQR;
    }

    internal static double GetUpperBound(IReadOnlyList<double> sortedData)
    {
        if (sortedData.Count == 0)
            return sortedData[0];

        var Q1 = GetPercentile(sortedData, 25);
        var Q3 = GetPercentile(sortedData, 75);
        var IQR = Q3 - Q1;
        return Q3 + 1.5 * IQR;
    }

    private static double GetPercentile(IReadOnlyList<double> sortedData, double percentile)
    {
        var N = sortedData.Count;
        var n = (N - 1) * percentile / 100.0 + 1;
        if (n == 1) return sortedData[0];
        if (n == N) return sortedData[N - 1];
        var k = (int)Math.Floor(n) - 1;
        var d = n - Math.Floor(n);
        return sortedData[k] + d * (sortedData[k + 1] - sortedData[k]);
    }
}
