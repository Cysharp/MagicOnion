using Benchmark.Client.Reports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Benchmark.Client
{
    public partial class BenchmarkReportPageTemplate
    {
        public HtmlBenchReport Report { get; init; }

        private static readonly string[] colorPatterns = new []
        {
              "#1f77b4",
              "#ff7f0e",
              "#2ca02c",
              "#d62728",
        };
        public string GetColor(int current) => colorPatterns[current % colorPatterns.Length];
    }
}
