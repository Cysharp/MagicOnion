using System;
using System.Collections.Generic;
using System.Net;
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

        public BenchReporter(string id)
        {
            _report = new BenchReport
            {
                Id = id,
                Client = Dns.GetHostName(),
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
