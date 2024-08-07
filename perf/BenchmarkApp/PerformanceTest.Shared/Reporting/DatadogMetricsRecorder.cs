using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PerformanceTest.Shared.Reporting;

// see: https://docs.datadoghq.com/api/latest/metrics/#submit-metrics
// spec:
// * 64 bits for the timestamp
// * 64 bits for the value
// * 20 bytes for the metric names
// * 50 bytes for the timeseries
// * The full payload is approximately 100 bytes.
public class DatadogMetricsRecorder
{
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly TimeProvider timeProvider = TimeProvider.System;
    private readonly HttpClient client;
    private readonly ConcurrentQueue<Task> reservations;

    private DatadogMetricsRecorder(string apiKey)
    {
        jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };
        jsonSerializerOptions.Converters.Add(new JsonNumberEnumConverter<DatadogMetricsType>());

        client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("DD-API-KEY", apiKey);

        reservations = new ConcurrentQueue<Task>();
    }

    public static DatadogMetricsRecorder Create(bool validate = false)
    {
        var apiKey = Environment.GetEnvironmentVariable("DD_API_KEY");
        if (validate)
        {
            ArgumentException.ThrowIfNullOrEmpty(apiKey);
        }
        return new DatadogMetricsRecorder(apiKey!);
    }

    /// <summary>
    /// Pass to background
    /// </summary>
    /// <param name="reserve"></param>
    public void Record(Task reserve)
    {
        reservations.Enqueue(reserve);
    }

    /// <summary>
    /// Wait until all records are posted
    /// </summary>
    /// <returns></returns>
    public async Task WaitSaveAsync()
    {
        // sequential handling to avoid Datadog API quota
        while (reservations.TryDequeue(out var task))
        {
            await task;
            if (reservations.Count == 0)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Send to Datadog
    /// </summary>
    /// <param name="metricsName"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    public async Task SendAsync(string metricsName, double value, DatadogMetricsType type, string[] tags)
    {
        var now = timeProvider.GetUtcNow().ToUnixTimeSeconds();
        var data = new DatadogMetricsRecord
        {
            Series = [
                new DatadogMetricsRecord.SeriesItem
                {
                    Metric = metricsName,
                    Type = type,
                    Points = [
                        new DatadogMetricsRecord.Point
                        {
                            Value = value,
                            Timestamp = now,
                        }
                    ],
                    Tags = tags,
                }
            ],
        };

        var json = JsonSerializer.Serialize(data, jsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.datadoghq.com/api/v2/series", content);

        // don't want to throw, show error message when failed instead
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to post metrics to Datadog. StatusCode: {(int)response.StatusCode}, Response: {responseContent}");
        }
    }
}

public static class DatadogMetricsRecorderExtensions
{
    /// <summary>
    /// Put Client Benchmark metrics to background. 
    /// </summary>
    /// <param name="recorder"></param>
    /// <param name="scenario"></param>
    /// <param name="applicationInfo"></param>
    /// <param name="serialization"></param>
    /// <param name="requestsPerSecond"></param>
    /// <param name="duration"></param>
    /// <param name="totalRequests"></param>
    public static void PutClientBenchmarkMetrics(this DatadogMetricsRecorder recorder, string scenario, ApplicationInformation applicationInfo, string serialization, double requestsPerSecond, TimeSpan duration, int totalRequests)
    {
        var tags = MetricsTagCache.Get((scenario, applicationInfo, serialization), static x => [$"magiconion_version:{x.applicationInfo.MagicOnionVersion}", $"grpcdotnet_version:{x.applicationInfo.GrpcNetVersion}", $"messagepack_version:{x.applicationInfo.MessagePackVersion}", $"memorypack_version:{x.applicationInfo.MemoryPackVersion}", $"process_arch:{x.applicationInfo.ProcessArchitecture}", $"process_count:{x.applicationInfo.ProcessorCount}", $"scenario:{x.scenario}", $"serialization:${x.serialization}"]);

        // Don't want to await each put. Let's send it to queue and await when benchmark ends.
        recorder.Record(recorder.SendAsync("benchmark.magiconion.rps", requestsPerSecond, DatadogMetricsType.Rate, tags)); // rate?
        recorder.Record(recorder.SendAsync("benchmark.magiconion.duration", requestsPerSecond, DatadogMetricsType.Gauge, tags)); // gauge?
        recorder.Record(recorder.SendAsync("benchmark.magiconion.total_requests", totalRequests, DatadogMetricsType.Gauge, tags));
    }
}

public class DatadogMetricsRecord
{
    [JsonPropertyName("series")]
    public required SeriesItem[] Series { get; set; }

    public class Point
    {
        /// <summary>
        /// The timestamp should be in seconds and current. Current is defined as not more than 10 minutes in the future or more than 1 hour in the past.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// The numeric value format should be a 64bit float gauge-type value.
        /// </summary>
        [JsonPropertyName("value")]
        public double Value { get; set; }
    }

    public class Resource
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The type of the resource.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class SeriesItem
    {
        /// <summary>
        /// If the type of the metric is rate or count, define the corresponding interval in seconds.
        /// </summary>
        [JsonPropertyName("internal")]
        public long? Interval { get; set; }

        /// <summary>
        /// The name of the timeseries.
        /// </summary>
        [JsonPropertyName("metric")]
        public required string Metric { get; set; }

        /// <summary>
        /// Points relating to a metric. All points must be objects with timestamp and a scalar value (cannot be a string). Timestamps should be in POSIX time in seconds, and cannot be more than ten minutes in the future or more than one hour in the past.
        /// </summary>
        [JsonPropertyName("points")]
        public required Point[] Points { get; set; }

        /// <summary>
        /// A list of resources to associate with this metric.
        /// </summary>
        [JsonPropertyName("resources")]
        public Resource[]? Resources { get; set; }

        /// <summary>
        /// A list of tags associated with the metric.
        /// </summary>
        public required string[] Tags { get; set; }

        /// <summary>
        /// The type of metric. 
        /// </summary>
        [JsonPropertyName("type")]
        public DatadogMetricsType Type { get; set; } = DatadogMetricsType.Count;

        /// <summary>
        /// The unit of point value.
        /// </summary>
        [JsonPropertyName("unit")]
        public string? Unit { get; init; }
    }
}

/// <summary>
/// The type of metric. The available types are 0 (unspecified), 1 (count), 2 (rate), and 3 (gauge). Allowed enum values: 0,1,2,3
/// </summary>
public enum DatadogMetricsType
{
    Unspecified = 0,
    Count = 1,
    Rate = 2,
    Gauge = 3,
}
