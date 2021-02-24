using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using System;
using System.Collections.Generic;

namespace Cdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new CdkStack(app, "MagicOnionBenchmarkCdkStack", new ReportStackProps
            {
                ForceRecreateMagicOnion = false,
                EnableCronScaleInEc2 = true,
                UseEc2CloudWatchAgentProfiler = true,
                UseEc2DatadogAgentProfiler = false,
                UseFargateDatadogAgentProfiler = true,
                MagicOnionInstanceType = InstanceType.Of(InstanceClass.COMPUTE5_AMD, InstanceSize.LARGE),
                MasterFargateSpec = new FargateSpec(CpuSpec.Half, MemorySpec.Low),
                WorkerFargateSpec = new FargateSpec(CpuSpec.Quater, MemorySpec.Low),
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
        /// <summary>
        /// Enable ScaleIn on 0:00:00 (UTC)
        /// </summary>
        public bool EnableCronScaleInEc2 { get; set; }
        /// <summary>
        /// Flag to force recreate MagicOnion Ec2 Server
        /// </summary>
        public bool ForceRecreateMagicOnion { get; set; }
        /// <summary>
        /// Execution time
        /// </summary>
        public DateTime ExecuteTime { get; set; }
        public string ReportId { get; set; }
        /// <summary>
        /// Number of days to keep reports in S3 bucket.
        /// </summary>
        public int DaysKeepReports { get; set; } = 7;
        public bool UseVersionedS3 { get; set; }
        /// <summary>
        /// Install CloudWatch Agent to EC2 MagicOnion and get MemoryUsage / TCP Established metrics.
        /// </summary>
        public bool UseEc2CloudWatchAgentProfiler { get; set; }
        /// <summary>
        /// Install Datadog Agent to EC2 MagicOnion.
        /// </summary>
        public bool UseEc2DatadogAgentProfiler { get; set; }
        /// <summary>
        /// Install Datadog Agent as Fargate sidecar container.
        /// </summary>
        public bool UseFargateDatadogAgentProfiler { get; set; }
        public InstanceType MagicOnionInstanceType { get; set; }
        /// <summary>
        /// Fargate Spec of Dframe master 
        /// </summary>
        public FargateSpec MasterFargateSpec { get; set; }
        /// <summary>
        /// Fargate Spec of Dframe worker
        /// </summary>
        public FargateSpec WorkerFargateSpec { get; set; }

        public ReportStackProps()
        {
            var now = DateTime.Now;
            ExecuteTime = now;
            ReportId = $"{now.ToString("yyyyMMdd-HHmmss")}-{Guid.NewGuid().ToString()}";
        }

        public static ReportStackProps ParseOrDefault(IStackProps props, ReportStackProps @default = null)
        {
            if (props is ReportStackProps r)
            {
                return r;
            }
            else
            {
                return @default != null ? @default : new ReportStackProps();
            }
        }
    }

    /// <summary>
    /// FargateSpec follow to the rule.
    /// https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-cpu-memory-error.html
    /// </summary>
    public class FargateSpec
    {
        /// <summary>
        /// Cpu Spec, default 256 = 0.25
        /// </summary>
        public CpuSpec Cpu { get; set; }
        /// <summary>
        /// Memory Spec, default 512 = 0.5GB
        /// </summary>
        public MemorySpec Memory { get; set; }
        /// <summary>
        /// Cpu Size to use
        /// </summary>
        public int CpuSize => _cpuSize;
        private int _cpuSize;
        /// <summary>
        /// Memory Size to use
        /// </summary>
        public int MemorySize => _memorysize;
        private int _memorysize;

        public FargateSpec() : this(CpuSpec.Quater, MemorySpec.Low)
        {
        }
        public FargateSpec(CpuSpec cpu, MemorySpec memory)
        {
            Cpu = cpu;
            Memory = memory;
            _cpuSize = (int)Cpu;
            _memorysize = CalculateMemorySize(Cpu, Memory);
        }

        public FargateSpec(CpuSpec cpu, int memorySize)
        {
            Cpu = cpu;
            Memory = MemorySpec.Custom;
            _cpuSize = (int)Cpu;
            _memorysize = CalculateMemorySize(memorySize);
        }

        private int CalculateMemorySize(CpuSpec cpu, MemorySpec memory) => (int)cpu * (int)memory;
        /// <summary>
        /// Memory Calculation for Custom MemorySize
        /// </summary>
        /// <param name="memorySize"></param>
        /// <returns></returns>
        private int CalculateMemorySize(int memorySize)
        {
            switch (Cpu)
            {
                case CpuSpec.Quater:
                case CpuSpec.Half:
                case CpuSpec.Single:
                    throw new ArgumentOutOfRangeException($"You must select CpuSpec of Double or Quadruple.");
                case CpuSpec.Double:
                    {
                        // 4096 < n < 16384, n can be increments of 1024
                        if (memorySize % 1024 != 0)
                            throw new ArgumentOutOfRangeException($"{nameof(memorySize)} must be increments of 1024.");
                        if (memorySize < _cpuSize * 2)
                            throw new ArgumentOutOfRangeException($"{nameof(memorySize)} too low, must be larger than {_cpuSize * 2}");
                        if (memorySize > _cpuSize * 4)
                            throw new ArgumentOutOfRangeException($"{nameof(memorySize)} too large, must be lower than {_cpuSize * 4}");
                    }
                    break;
                case CpuSpec.Quadruple:
                    {
                        // 8192 < n < 30720, n can be increments of 1024
                        if (memorySize % 1024 != 0)
                            throw new ArgumentOutOfRangeException($"{nameof(memorySize)} must be increments of 1024.");
                        if (memorySize < _cpuSize * 2)
                            throw new ArgumentOutOfRangeException($"{nameof(memorySize)} too low, must be larger than {_cpuSize * 2}");
                        if (memorySize > _cpuSize * 7.5)
                            throw new ArgumentOutOfRangeException($"{nameof(memorySize)} too large, must be lower than {_cpuSize * 7.5}");
                    }
                    break;
            }
            return memorySize;
        }
    }

    public enum CpuSpec
    {
        Quater = 256,
        Half = 512,
        Single = 1024,
        Double = 2048,
        Quadruple = 4096,
    }

    public enum MemorySpec
    {
        /// <summary>
        /// Only available when CpuSpec is Double or Quadruple.
        /// </summary>
        Custom = 0,
        Low = 2,
        Medium = 4,
        High = 8,
    }
}
