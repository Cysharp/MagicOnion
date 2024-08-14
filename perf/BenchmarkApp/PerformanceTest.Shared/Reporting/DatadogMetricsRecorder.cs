using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PerformanceTest.Shared.Reporting;

public class DatadogMetricsRecorder
{
    public string TagLegend { get; }
    public string TagStreams { get; }
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly TimeProvider timeProvider;
    private readonly HttpClient client;
    private readonly ConcurrentQueue<Task> backgroundQueue;

    private DatadogMetricsRecorder(string tagLegend, string tagStreams, string apiKey, TimeProvider timeProvider)
    {
        TagLegend = tagLegend;
        TagStreams = tagStreams;
        jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };
        jsonSerializerOptions.Converters.Add(new JsonNumberEnumConverter<DatadogMetricsType>());
        this.timeProvider = timeProvider;

        client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("DD-API-KEY", apiKey);

        backgroundQueue = new ConcurrentQueue<Task>();
    }

    public static DatadogMetricsRecorder Create(string? tagString, bool validate = false)
    {
        string tagLegend = "";
        string tagStreams = "";
        if (!string.IsNullOrEmpty(tagString))
        {
            foreach (var item in tagString.Split(","))
            {
                if (item.StartsWith("legend:"))
                {
                    var index = item.IndexOf(':') + 1;
                    tagLegend = item.Substring(index);
                }
                else if (item.StartsWith("streams:"))
                {
                    var index = item.IndexOf(':') + 1;
                    tagStreams = item.Substring(index);
                }
            }
        }
        var apiKey = Environment.GetEnvironmentVariable("DD_API_KEY");
        if (validate)
        {
            ArgumentException.ThrowIfNullOrEmpty(apiKey);
        }
        return new DatadogMetricsRecorder(branch, tagLegend, tagStreams, apiKey!, SystemTimeProvider.TimeProvider);
    }

    /// <summary>
    /// Pass task to background
    /// </summary>
    /// <param name="task"></param>
    public void Record(Task task)
    {
        backgroundQueue.Enqueue(task);
    }

    /// <summary>
    /// Wait until all records are posted
    /// </summary>
    /// <returns></returns>
    public async Task WaitSaveAsync()
    {
        // sequential handling to avoid Datadog API quota
        while (backgroundQueue.TryDequeue(out var task))
        {
            await task;
            if (backgroundQueue.Count == 0)
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
    /// <param name="unit"></param>
    /// <returns></returns>
    public async Task SendAsync(string metricsName, double value, DatadogMetricsType type, string[] tags, string? unit = null)
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
                    Unit = unit,
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

// see: https://docs.datadoghq.com/api/latest/metrics/#submit-metrics
// spec:
// * 64 bits for the timestamp
// * 64 bits for the value
// * 20 bytes for the metric names
// * 50 bytes for the timeseries
// * The full payload is approximately 100 bytes.
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
        [JsonPropertyName("tags")]
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
