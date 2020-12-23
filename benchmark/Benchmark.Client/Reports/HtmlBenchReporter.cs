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
            var unaryItems = reports.SelectMany(x => x.Items).Where(x => x.Type == nameof(Grpc.Core.MethodType.Unary)).ToArray();
            var hubItems = reports.SelectMany(x => x.Items).Where(x => x.Type == nameof(Grpc.Core.MethodType.DuplexStreaming)).ToArray();

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
                Id = reports.Select(x => x.ReportId).First(),
                Clients = reports.Length,
                Itelations = reports.SelectMany(xs => xs.Items.Select(x => x.RequestCount)).Sum(),
                Begin = reports.Select(x => x.Begin).OrderBy(x => x).First(),
                End = reports.Select(x => x.End).OrderByDescending(x => x).First(),
                DurationTotal = SumTimeSpan(durations),
                DurationAvg = AverageTimeSpan(durations),
                DurationMax = MaxTimeSpan(durations),
                DurationMin = MinTimeSpan(durations),
            };
            var unaryConnectionsResultInfo = new HtmlBenchReportConnectionsResult
            {
                SummaryItems = ConnectionAverage(unaryItems.Where(x => x.RequestCount != 0).OrderBy(x => x.RequestCount)),
                Errors = unaryItems.Sum(x => x.Errors),
                ClientDurationItems = unaryItems.OrderBy(x => x.Begin)
                        .GroupBy(x => x.ExecuteId)
                        .Select(xs => (Client: xs.Select(x => x.Client).First(), Items: xs
                            .Where(x => x.RequestCount != 0)
                            .Select(x => new HtmlBenchReportRequestsDurationItem
                            {
                                RequestCount = x.RequestCount,
                                Duration = x.Duration,
                                Rps = x.RequestCount / x.Duration.TotalSeconds,
                            })
                            .OrderBy(xs => xs.RequestCount)
                            .ToArray())
                        )
                        .ToArray(),
            };
            var hubConnectionsResultInfo = new HtmlBenchReportConnectionsResult
            {
                SummaryItems = ConnectionAverage(hubItems.Where(x => x.RequestCount != 0)),
                Errors = hubItems.Sum(x => x.Errors),
                ClientDurationItems = hubItems.OrderBy(x => x.Begin)
                        .GroupBy(x => x.ExecuteId)
                        .Select(xs => (Client: xs.Select(x => x.Client).First(), Items: xs
                            .Where(x => x.RequestCount != 0)
                            .Select(x => new HtmlBenchReportRequestsDurationItem
                            {
                                RequestCount = x.RequestCount,
                                Duration = x.Duration,
                                Rps = x.RequestCount / x.Duration.TotalSeconds,
                            })
                            .OrderBy(xs => xs.RequestCount)
                            .ToArray())
                        )
                        .ToArray(),
            };

            return new HtmlBenchReport(clientInfo, summaryInfo, unaryConnectionsResultInfo, hubConnectionsResultInfo);
        }

        private HtmlBenchReportRequestsDurationItem[] ConnectionAverage(IEnumerable<BenchReportItem> sources)
        {
            // { connections int: duration TimeSpan}
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            return sources.GroupBy(x => x.RequestCount)
                .Select(xs => (requests: xs.Key, duration: AverageTimeSpan(xs.Select(x => x.Duration))))
                .Select(x => new HtmlBenchReportRequestsDurationItem
                {
                    RequestCount = x.requests,
                    Duration = x.duration,
                    Rps = x.requests / x.duration.TotalSeconds,
                })
                .ToArray();
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

        private static string ToJoinedString(IEnumerable<string> values, char separator = ',')
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (!values.Any())
                return "";
            return string.Join(separator, values);
        }
    }
}
