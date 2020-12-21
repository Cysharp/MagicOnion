using System;
using System.Collections.Generic;

namespace Benchmark.Client.Reports
{
    public record HtmlBenchReport(HtmlBenchReportClientInfo Client, HtmlBenchReportSummary Summary, HtmlBenchReportResult Result);
    public record HtmlBenchReportClientInfo
    {
        public string Os { get; init; }
        public string Architecture { get; init; }
        public int Processors { get; init; }
        public long Memory { get; init; }
        public string Framework { get; init; }
        public string Version { get; init; }
    }
    public record HtmlBenchReportSummary
    {
        public string Id { get; init; }
        public int Clients { get; init; }
        public int Itelations { get; init; }
        public DateTime Begin { get; init; }
        public DateTime End { get; init; }
        public TimeSpan DurationTotal { get; init; }
        public TimeSpan DurationAvg { get; init; }
        public TimeSpan DurationMin { get; init; }
        public TimeSpan DurationMax { get; init; }
    }
    public record HtmlBenchReportResult
    {
        public Dictionary<int, TimeSpan> BenchItems  { get; init; }
        public int Errors { get; init; }
    }
}
