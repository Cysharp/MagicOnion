using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Benchmark.ClientLib.Reports
{
    public class BenchReport
    {
        [JsonPropertyName("report_id")]
        public string ReportId { get; set; }
        [JsonPropertyName("execute_id")]
        public string ExecuteId { get; set; }
        [JsonPropertyName("client")]
        public string Client { get; set; }
        [JsonPropertyName("os")]
        public string OS { get; set; }
        [JsonPropertyName("os_architecture")]
        public string OsArchitecture { get; set; }
        [JsonPropertyName("process_architecture")]
        public string ProcessArchitecture { get; set; }
        [JsonPropertyName("cpu_number")]
        public int CpuNumber { get; set; }
        [JsonPropertyName("system_memory")]
        public long SystemMemory { get; set; }
        [JsonPropertyName("framework")]
        public string Framework { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("begin")]
        public DateTime Begin { get; set; }
        [JsonPropertyName("end")]
        public DateTime End { get; set; }
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }
        [JsonPropertyName("items")]
        public BenchReportItem[] Items { get; set; }
    }

    public class BenchReportItem
    {
        [JsonPropertyName("execute_id")]
        public string ExecuteId { get; set; }
        [JsonPropertyName("client")]
        public string Client { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("test_name")]
        public string TestName { get; set; }
        [JsonPropertyName("begin")]
        public DateTime Begin { get; set; }
        [JsonPropertyName("end")]
        public DateTime End { get; set; }
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }
        [JsonPropertyName("request_count")]
        public int RequestCount { get; set; }
        [JsonPropertyName("error_count")]
        public int Errors { get; set; }
    }
}
