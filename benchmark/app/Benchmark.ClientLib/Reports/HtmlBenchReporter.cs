using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Reports
{
    public class HtmlBenchReporter
    {
        public HtmlBenchReport CreateReport(BenchReport[] reports, bool generateDetail)
        {
            if (reports == null)
                throw new ArgumentNullException(nameof(reports));
            if (!reports.Any())
                throw new ArgumentException($"{nameof(reports)} not contains any element.");

            var requests = reports.SelectMany(xs => xs.Items.Select(x => x.RequestCount)).Sum();
            var durations = reports.Select(x => x.Duration).ToArray();
            var unaryItems = reports.SelectMany(x => x.Items).Where(x => x.Type == nameof(Grpc.Core.MethodType.Unary)).ToArray();
            var hubItems = reports.SelectMany(x => x.Items).Where(x => x.Type == nameof(Grpc.Core.MethodType.DuplexStreaming)).ToArray();

            var client = new HtmlBenchReportClientInfo
            {
                Os = ToJoinedString(reports.Select(x => x.OS).Distinct()),
                Architecture = ToJoinedString(reports.Select(x => x.OS).Distinct()),
                Processors = reports.Select(x => x.CpuNumber).Distinct().OrderByDescending(x => x).First(),
                Memory = reports.Select(x => x.SystemMemory).Distinct().OrderByDescending(x => x).First(), // take biggest
                Framework = ToJoinedString(reports.Select(x => x.Framework).Distinct()),
                Version = ToJoinedString(reports.Select(x => x.Version).Distinct()),
            };
            var summary = new HtmlBenchReportSummary
            {
                ReportId = reports.Select(x => x.ReportId).First(),
                Clients = reports.GroupBy(x => x.ClientId).Count(),
                RequestTotal = requests,
                Begin = reports.Select(x => x.Begin).OrderBy(x => x).First(),
                End = reports.Select(x => x.End).OrderByDescending(x => x).First(),
                Rps = requests / SumTimeSpan(durations).TotalSeconds,
                DurationTotal = SumTimeSpan(durations),
                DurationAvg = AverageTimeSpan(durations),
                DurationMax = MaxTimeSpan(durations),
                DurationMin = MinTimeSpan(durations),
            };
            var unaryClientResult = new HtmlBenchReportClientResult
            {
                SummaryItems = GetClientSummaryItems(unaryItems.Where(x => x.RequestCount != 0)
                    .OrderBy(x => x.RequestCount)),
                Errors = unaryItems.Sum(x => x.Errors),
                ClientDurationItems = generateDetail
                    ? unaryItems.OrderBy(x => x.Begin)
                        .GroupBy(x => (x.ExecuteId, x.ClientId))
                        .Select(xs => (Client: xs.Key.ClientId, Items: xs
                            .Where(x => x.RequestCount != 0)
                            .GroupBy(x => x.ClientId)
                                .Select(xs =>
                                {
                                    return new HtmlBenchReportClientResultClientDurationItem
                                    {
                                        ClientCount = xs.Count(),
                                        SummaryItems = xs.Select(x =>
                                        {
                                            return new HtmlBenchReportClientResultSummaryItem
                                            {
                                                RequestCount = x.RequestCount,
                                                Duration = x.Duration,
                                                Rps = x.RequestCount / x.Duration.TotalSeconds,
                                            };
                                        })
                                        .ToArray(),
                                    };
                                })
                                .OrderBy(x => x.ClientCount)
                                .ToArray()
                            )
                        )
                        .ToArray()
                    : Array.Empty<(string, HtmlBenchReportClientResultClientDurationItem[])>(),
            };
            var unaryRequestResult = new HtmlBenchReportRequestResult
            {
                SummaryItems = GetRequestSummaryItems(unaryItems.Where(x => x.RequestCount != 0).OrderBy(x => x.RequestCount)),
                Errors = unaryItems.Sum(x => x.Errors),
                ClientDurationItems = generateDetail 
                    ? unaryItems.OrderBy(x => x.Begin)
                        .GroupBy(x => (x.ExecuteId, x.ClientId))
                        .Select(xs => (Client: xs.Key.ClientId, Items: xs
                            .Where(x => x.RequestCount != 0)
                            .GroupBy(x => x.RequestCount)
                                .Select(xs =>
                                {
                                    return new HtmlBenchReportRequestResultClientDurationItem
                                    {
                                        RequestCount = xs.Key,
                                        SummaryItems = xs.Select(x =>
                                        {
                                            return new HtmlBenchReportRequestResultSummaryItem
                                            {
                                                RequestCount = x.RequestCount,
                                                Duration = x.Duration,
                                                Rps = x.RequestCount / x.Duration.TotalSeconds,
                                            };
                                        })
                                        .ToArray(),
                                    };
                                })
                                .OrderBy(x => x.RequestCount)
                                .ToArray()
                            )
                        )
                        .ToArray()
                    : Array.Empty<(string, HtmlBenchReportRequestResultClientDurationItem[])>(),
            };
            var grps = hubItems.OrderBy(x => x.Begin).GroupBy(x => x.RequestCount).ToArray();
            var hubRequestResult = new HtmlBenchReportRequestResult
            {
                SummaryItems = GetRequestSummaryItems(hubItems.Where(x => x.RequestCount != 0).OrderBy(x => x.RequestCount)),
                Errors = hubItems.Sum(x => x.Errors),
                ClientDurationItems = generateDetail
                    ? hubItems.OrderBy(x => x.Begin)
                            .GroupBy(x => (x.ExecuteId, x.ClientId))
                            .Select(xs => (Client: xs.Key.ClientId, Items: xs
                                .Where(x => x.RequestCount != 0)
                                .GroupBy(x => x.RequestCount)
                                    .Select(xs =>
                                    {
                                        return new HtmlBenchReportRequestResultClientDurationItem
                                        {
                                            RequestCount = xs.Key,
                                            SummaryItems = xs.Select(x =>
                                            {
                                                return new HtmlBenchReportRequestResultSummaryItem
                                                {
                                                    RequestCount = x.RequestCount,
                                                    Duration = x.Duration,
                                                    Rps = x.RequestCount / x.Duration.TotalSeconds,
                                                };
                                            })
                                            .ToArray(),
                                        };
                                    })
                                    .OrderBy(x => x.RequestCount)
                                    .ToArray()
                                )
                            )
                            .ToArray()
                    : Array.Empty<(string, HtmlBenchReportRequestResultClientDurationItem[])>(),
            };

            return new HtmlBenchReport(
                client, 
                summary,
                unaryClientResult,
                unaryRequestResult, 
                hubRequestResult);
        }

        private HtmlBenchReportClientResultSummaryItem[] GetClientSummaryItems(IEnumerable<BenchReportItem> sources)
        {
            // { connections int: duration TimeSpan}
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            // todo: client group by?
            return sources.GroupBy(x => x.RequestCount)
                .Select(xs =>
                {
                    var duration = SumTimeSpan(xs.Select(x => x.Duration));
                    return new HtmlBenchReportClientResultSummaryItem
                    {
                        RequestCount = xs.Key,
                        Duration = AverageTimeSpan(xs.Select(x => x.Duration)),
                        Rps = xs.Select(x => x.RequestCount).Sum() / duration.TotalSeconds,
                    };
                })
                .ToArray();
        }
        private HtmlBenchReportRequestResultSummaryItem[] GetRequestSummaryItems(IEnumerable<BenchReportItem> sources)
        {
            // { connections int: duration TimeSpan}
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            return sources.GroupBy(x => x.RequestCount)
                .Select(xs =>
                {
                    var duration = SumTimeSpan(xs.Select(x => x.Duration));
                    return new HtmlBenchReportRequestResultSummaryItem
                    {
                        RequestCount = xs.Key,
                        Duration = AverageTimeSpan(xs.Select(x => x.Duration)),
                        Rps = xs.Select(x => x.RequestCount).Sum() / duration.TotalSeconds,
                    };
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
