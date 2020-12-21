using Benchmark.Client.Reports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Benchmark.Client
{
    public partial class BenchmarkReportPageTemplate : BenchmarkReportPageTemplateBase
    {
        public HtmlBenchReport Report { get; init; }
    }
}
