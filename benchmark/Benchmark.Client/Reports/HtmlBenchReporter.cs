using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Client.Reports
{
    public class HtmlBenchReporter
    {
        public HtmlBenchReport CreateReport(BenchReport[] reports)
        {
            if (reports == null)
                throw new ArgumentNullException(nameof(reports));
            if (!reports.Any())
                throw new ArgumentException($"{nameof(reports)} not contains any element.");

            var durations = reports.Select(x => x.Duration).ToArray();

            var clientInfo = new HtmlBenchReportClientInfo
            {
                Os = ToJoinedString(reports.Select(x => x.OS).Distinct()),
                Architecture = ToJoinedString(reports.Select(x => x.OS).Distinct()),
                Processors = reports.Select(x => x.CpuNumber).Distinct().OrderByDescending(x => x).First(),
                Memory = reports.Select(x => x.SystemMemory).Distinct().OrderByDescending(x => x).First(), // take biggest
                Framework = "MagicOnion",
                Version = typeof(MagicOnion.IServiceMarker).Assembly.GetName().Version.ToString(),
            };
            var summaryInfo = new HtmlBenchReportSummary
            {
                Id = reports.Select(x => x.Id).First(),
                Clients = reports.Length,
                Itelations = reports.SelectMany(xs => xs.Items.Select(x => x.RequestCount)).Sum(),
                Begin = reports.Select(x => x.Begin).OrderBy(x => x).First(),
                End = reports.Select(x => x.End).OrderByDescending(x => x).First(),
                DurationTotal = SumTimeSpan(durations),
                DurationAvg = AverageTimeSpan(durations),
                DurationMax = MaxTimeSpan(durations),
                DurationMin = MinTimeSpan(durations),
            };
            var resultInfo = new HtmlBenchReportResult
            {
            };

            return new HtmlBenchReport(clientInfo, summaryInfo, resultInfo);
        }

        private static TimeSpan SumTimeSpan(IEnumerable<TimeSpan> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (!sources.Any())
                return TimeSpan.Zero;

            var sum = TimeSpan.Zero;
            foreach (var source in sources)
            {
                sum += source;
            }
            return sum;
        }
        private static TimeSpan AverageTimeSpan(IEnumerable<TimeSpan> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (!sources.Any())
                return TimeSpan.Zero;

            var sum = SumTimeSpan(sources);
            var avg = sum / sources.Count();
            return avg;
        }
        private static TimeSpan MaxTimeSpan(IEnumerable<TimeSpan> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (!sources.Any())
                return TimeSpan.Zero;

            var max = TimeSpan.Zero;
            foreach (var source in sources)
            {
                if (source > max)
                    max = source;
            }
            return max;
        }
        private static TimeSpan MinTimeSpan(IEnumerable<TimeSpan> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (!sources.Any())
                return TimeSpan.Zero;

            TimeSpan? min = null;
            foreach (var source in sources)
            {
                if (min == null)
                {
                    min = source;
                    break;
                }

                if (source < min)
                    min = source;
            }
            return min.Value;
        }

        private string ToJoinedString(IEnumerable<string> values, char separator = ',')
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (!values.Any())
                return "";
            return string.Join(separator, values);
        }
    }
}
