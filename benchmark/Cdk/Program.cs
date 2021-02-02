using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new CdkStack(app, "MagicOnionBenchmarkCdkStack", new ReportStackProps
            {
                Tags = new Dictionary<string, string>()
                {
                    { "environment", "bench" },
                    { "cf-stack", "MagicOnionBenchmarkCdkStack" },
                },
            });
            app.Synth();
        }
    }

    public class ReportStackProps : StackProps
    {
        public DateTime ExecuteTime { get; set; }
        public string ReportId { get; set; }

        public ReportStackProps()
        {
            var now = DateTime.Now;
            ExecuteTime = now;
            ReportId = $"{now.ToString("yyyyMMddHHmmss")}-{Guid.NewGuid().ToString()}";
        }
    }
}
