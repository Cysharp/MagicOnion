using System.Buffers;

namespace PerformanceTest.Shared.Reporting;

/// <summary>
/// Outlier removal by IQR (Interquartile Range) method. Falls back to MAD (Median Absolute Deviation) if IQR is zero.
/// </summary>
public static class OutlierIqr
{
    /// <summary>
    /// Remove outliers from data using IQR method.
    /// </summary>
    /// <param name="data">The input data. This span will be sorted in-place.</param>
    /// <param name="upperLimit">
    /// The maximum allowed value for inliers. This value acts as an additional constraint 
    /// on the upper bound calculated by the IQR method (Q3 + 1.5 × IQR). 
    /// The effective upper bound will be the minimum of the IQR-calculated bound and this limit.
    /// Use <see cref="double.PositiveInfinity"/> if no additional upper constraint is needed.
    /// </param>
    /// <returns>A range representing the valid data slice after outlier removal.</returns>
    public static Range FindInlierRange(Span<double> data, double upperLimit)
    {
        if (data.Length == 0)
            return 0..0;

        data.Sort();

        GetQuartiles(data, out var q1, out var q3);
        var iqr = q3 - q1;

        double lowerBound, upperBound;

        // If IQR is almost zero, fall back to MAD method.
        double scale = Math.Max(Math.Max(Math.Abs(q1), Math.Abs(q3)), 1.0);
        double tolerance = 1e-12 * scale;
        if (iqr <= tolerance)
        {
            // IQR can be zero if all data points are identical or very close to each other.
            // This weakness of IQR method can be mitigated by using MAD.
            double mad = CalculateMAD(data, out var median);

            // All data points are identical; return the full range.
            double medianScale = Math.Max(Math.Abs(median), 1.0);
            double madTolerance = 1e-12 * medianScale;
            if (mad <= madTolerance)
                return 0..data.Length;

            // Use MAD to determine bounds (median ± 2.5 * MAD is approximately equivalent to IQR 1.5)
            const double madScale = 1.4826; // 1 / 0.6745, consistency with Normal
            const double k = 3.0; // ~3σ rule
            var scaledMAD = mad * madScale;
            lowerBound = median - k * scaledMAD;
            upperBound = median + k * scaledMAD;
        }
        else
        {
            // Standard IQR method
            lowerBound = q1 - 1.5 * iqr;
            upperBound = q3 + 1.5 * iqr;
        }

        var effectiveUpperBound = Math.Min(upperBound, upperLimit);

        int startIndex = BinarySearchLowerBound(data, lowerBound);
        int endIndex = BinarySearchUpperBound(data, effectiveUpperBound);

        return startIndex..endIndex;
    }

    /// <summary>
    /// Calculate the lower bound for outlier detection using IQR method (Q1 - 1.5 × IQR).
    /// </summary>
    /// <param name="sortedData">The sorted data.</param>
    /// <returns>The lower bound value.</returns>
    public static double GetLowerBound(ReadOnlySpan<double> sortedData)
    {
        GetQuartiles(sortedData, out var q1, out var q3);
        var iqr = q3 - q1;
        return q1 - 1.5 * iqr;
    }

    /// <summary>
    /// Calculate the upper bound for outlier detection using IQR method (Q3 + 1.5 × IQR).
    /// </summary>
    /// <param name="sortedData">The sorted data.</param>
    /// <returns>The upper bound value.</returns>
    public static double GetUpperBound(ReadOnlySpan<double> sortedData)
    {
        GetQuartiles(sortedData, out var q1, out var q3);
        var iqr = q3 - q1;
        return q3 + 1.5 * iqr;
    }

    /// <summary>
    /// Calculate Median Absolute Deviation (MAD) from sorted data.
    /// </summary>
    /// <remarks>
    /// MAD = median(|x_i - median(x)|)
    /// <para>
    /// MAD has a breakdown point of 50%, meaning it can tolerate up to 50% of the data being outliers.
    /// However, when more than 50% of data points are identical or very close to the median, MAD ≈ 0,
    /// making outlier detection impossible.
    /// </para>
    /// </remarks>
    private static double CalculateMAD(ReadOnlySpan<double> sortedData, out double median)
    {
        // Calculate median
        median = GetPercentile(sortedData, 50.0);

        if (sortedData.Length == 1)
            return 0.0;

        // Calculate |x_i - median|
        double[]? tmpDeviations = null;
        try
        {
            Span<double> deviations = sortedData.Length <= 1024
                ? stackalloc double[sortedData.Length]
                : (tmpDeviations = ArrayPool<double>.Shared.Rent(sortedData.Length)).AsSpan(0, sortedData.Length);

            for (int i = 0; i < sortedData.Length; i++)
            {
                deviations[i] = Math.Abs(sortedData[i] - median);
            }

            // Sort the deviations to find the median
            deviations.Sort();
            return GetPercentile(deviations, 50.0);
        }
        finally
        {
            if (tmpDeviations is not null)
                ArrayPool<double>.Shared.Return(tmpDeviations);
        }
    }

    private static int BinarySearchLowerBound(ReadOnlySpan<double> sortedData, double value)
    {
        int left = 0;
        int right = sortedData.Length;
        while (left < right)
        {
            int mid = left + (right - left) / 2;
            if (sortedData[mid] < value)
                left = mid + 1;
            else
                right = mid;
        }
        return left;
    }

    private static int BinarySearchUpperBound(ReadOnlySpan<double> sortedData, double value)
    {
        int left = 0;
        int right = sortedData.Length;
        while (left < right)
        {
            int mid = left + (right - left) / 2;
            if (sortedData[mid] <= value)
                left = mid + 1;
            else
                right = mid;
        }
        return left;
    }

    internal static void GetQuartiles(ReadOnlySpan<double> sortedData, out double q1, out double q3)
    {
        q1 = GetPercentile(sortedData, 25.0);
        q3 = GetPercentile(sortedData, 75.0);
    }

    private static double GetPercentile(ReadOnlySpan<double> sortedData, double p)
    {
        int n = sortedData.Length;
        if (n == 1) return sortedData[0];

        double pos = (n - 1) * (p / 100.0);
        int k = (int)pos;
        double frac = pos - k;

        if (k >= n - 1) return sortedData[n - 1];
        return sortedData[k] + (sortedData[k + 1] - sortedData[k]) * frac;
    }
}
