using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Benchmark.Client.Reports
{
    public class BenchReporter
    {
        private readonly BenchReport _report;
        private readonly List<BenchReportItem> _items;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public string Id { get; }
        public string Name { get; }

        public BenchReporter(string id, string name)
        {
            Id = id;
            Name = name;
            _report = new BenchReport
            {
                Id = id,
                Client = name,
            };
            _items = new List<BenchReportItem>();
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // allow Unicode characters
                WriteIndented = true, // prety print
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // ignore null                
            };
        }

        /// <summary>
        /// Get Report
        /// </summary>
        /// <returns></returns>
        public BenchReport GetReport()
        {
            return _report;
        }

        /// <summary>
        /// Add inidivisual Bench Report Detail
        /// </summary>
        /// <param name="item"></param>
        public void AddBenchDetail(BenchReportItem item)
        {
            _items.Add(item);
            _report.DurationMs = _items.Sum(x => x.DurationMs);
            _report.Items = _items.ToArray();
        }

        /// <summary>
        /// Output report as a json
        /// </summary>
        /// <returns></returns>
        public string OutputJson()
        {
            var currentReport = GetReport();
            var json = JsonSerializer.Serialize(currentReport, _jsonSerializerOptions);
            return json;
        }
    }
}
