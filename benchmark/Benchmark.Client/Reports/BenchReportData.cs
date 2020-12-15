using System;
using System.Text.Json.Serialization;

namespace Benchmark.Client.Reports
{
    public class BenchReport
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("client")]
        public string Client { get; set; }
        [JsonPropertyName("duration_ms")]
        public double DurationMs { get; set; }
        [JsonPropertyName("items")]
        public BenchReportItem[] Items { get; set; }
    }

    public class BenchReportItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("test_name")]
        public string TestName { get; set; }
        [JsonPropertyName("begin")]
        public DateTime Begin { get; set; }
        [JsonPropertyName("end")]
        public DateTime End { get; set; }
        [JsonPropertyName("duration_ms")]
        public double DurationMs { get; set; }
        [JsonPropertyName("request_count")]
        public int RequestCount { get; set; }
    }
}
