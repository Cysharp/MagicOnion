using Benchmark.ClientLib.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Benchmark.ClientLib.Reports
{
    public class BenchReporter
    {
        private readonly BenchReport _report;
        private readonly List<BenchReportItem> _items;

        public string ReportId { get; }
        public string Name { get; }
        public string ExecuteId { get; }

        public BenchReporter(string reportId, string executeId, string name, string framework = "MagicOnion")
        {
            ReportId = reportId;
            Name = name;
            ExecuteId = executeId;

            _report = new BenchReport
            {
                ReportId = reportId,
                ExecuteId = executeId,
                Client = name,
                OS = RuntimeInformation.OSDescription,
                OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                CpuNumber = Environment.ProcessorCount,
                SystemMemory = GetSystemMemory(),
                Framework = framework,
                Version = typeof(MagicOnion.IServiceMarker).Assembly.GetName().Version?.ToString(),
            };
            _items = new List<BenchReportItem>();
        }

        /// <summary>
        /// Get Report
        /// </summary>
        /// <returns></returns>
        public BenchReport GetReport()
        {
            return _report;
        }

        public void Begin()
        {
            _report.Begin = DateTime.UtcNow;
        }

        public void End()
        {
            _report.End = DateTime.UtcNow;
            _report.Duration = _report.End - _report.Begin;
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
        public string ToJson()
        {
            return JsonConvert.Serialize(_report);
        }

        public string GetJsonFileName()
        {
            return Name + "-" + ExecuteId + ".json";
        }

        /// <summary>
        /// System Memory for GB
        /// </summary>
        /// <returns></returns>
        private static long GetSystemMemory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows require wmic and priviledged. lets omit
                return 0;
            }
            else
            {
                // sample data (64GB machine)
                // MemTotal:        6110716 kB
                var memTotal = System.IO.File.ReadAllLines("/proc/meminfo").FirstOrDefault();
                var lines = memTotal.Split("MemTotal:");
                if (lines.Length == 2)
                {
                    var memString = lines[1].Split("kb")[0];
                    if (double.TryParse(memString.Trim(), out var memory))
                    {
                        return (long)Math.Floor(memory / 1024 / 1024);
                    }
                }
                return 0;
            }
        }
    }
}
