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
                Endpoint = Endpoint.ServiceDiscoveryWithHttp,
                AlbDomain = ("dev.cysharp.io", "Z075519318R3LY1VXMWII"),
                ForceRecreateMagicOnion = false,
                EnableCronScaleInEc2 = true,
                UseEc2CloudWatchAgentProfiler = true,
                UseEc2DatadogAgentProfiler = false,
                UseFargateDatadogAgentProfiler = true,
                MagicOnionInstanceType = InstanceType.Of(InstanceClass.COMPUTE5_AMD, InstanceSize.LARGE),
                MasterFargate = new Fargate(Fargate.CpuSpec.Half, Fargate.MemorySpec.Low),
                WorkerFargate = new Fargate(Fargate.CpuSpec.Double, Fargate.MemorySpec.Low),
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
        /// EndpointStyle to select BenchCommnicationStyle
        /// </summary>
        public Endpoint Endpoint { get; set; } = Endpoint.ServiceDiscoveryWithHttp;
        /// <summary>
        /// Register AlbDomain on AlbMode
        /// </summary>
        public (string domain, string zoneId) AlbDomain { get; set; }
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
        /// <summary>
        /// ReportId
        /// </summary>
        public string ReportId { get; set; }
        /// <summary>
        /// Number of days to keep reports in S3 bucket.
        /// </summary>
        public int DaysKeepReports { get; set; } = 3;
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
        /// Fargate of Dframe master 
        /// </summary>
        public Fargate MasterFargate { get; set; }
        /// <summary>
        /// Fargate of Dframe worker
        /// </summary>
        public Fargate WorkerFargate { get; set; }

        public ReportStackProps()
        {
            var now = DateTime.Now;
            ExecuteTime = now;
            ReportId = $"{now.ToString("yyyyMMdd-HHmmss")}-{Guid.NewGuid().ToString()}";
        }

        public BenchCommunicationStyle GetBenchCommunicationStyle()
        {
            return new BenchCommunicationStyle(Endpoint);
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

    public enum Endpoint
    {
        /// <summary>
        /// Worker to Insecure MagicOnion with ServiceDiscovery, gRPC over Insecure TLS
        /// </summary>
        ServiceDiscoveryWithHttp = 0,
        /// <summary>
        /// Worker to HTTPS MagicOnion with ServiceDiscovery, gRPC over TLS
        /// </summary>
        ServiceDiscoveryWithHttps,
        /// <summary>
        /// Worker to HTTP MagicOnion with ALB (HTTPS), gRPC over TLS.
        /// </summary>
        Alb,
    }

    public class BenchCommunicationStyle
    {
        /// <summary>
        /// MagicOnion - Benchmarker communication style. default: ServiceDiscovery.
        /// </summary>
        public CommunicationType Type { get; }
        /// <summary>
        /// Indicate Alb is required.
        /// </summary>
        public bool RequireAlb => Type == CommunicationType.Alb;
        /// <summary>
        /// Listen MagicOnion on Https
        /// </summary>
        public bool ListenMagicOnionTls { get; }
        /// <summary>
        /// Bench worker to target Https MagicOnion Endpoint
        /// </summary>
        public bool UseHttpsEndpoint { get; }
        public string EndpointSchema => UseHttpsEndpoint ? "https" : "http";

        public BenchCommunicationStyle(Endpoint endpointStyle)
        {
            switch (endpointStyle)
            {
                case Endpoint.ServiceDiscoveryWithHttp:
                    Type = CommunicationType.ServiceDiscovery;
                    ListenMagicOnionTls = false;
                    UseHttpsEndpoint = false;
                    break;
                case Endpoint.ServiceDiscoveryWithHttps:
                    Type = CommunicationType.ServiceDiscovery;
                    ListenMagicOnionTls = true;
                    UseHttpsEndpoint = true;
                    break;
                case Endpoint.Alb:
                    Type = CommunicationType.Alb;
                    ListenMagicOnionTls = false;
                    UseHttpsEndpoint = true;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// MagicOnion - Benchmarker communication style. default: ServiceDiscovery.
        /// </summary>
        public enum CommunicationType
        {
            /// <summary>
            /// Use Service Discovery. (You can choose Non-TLS or TLS)
            /// </summary>
            ServiceDiscovery = 0,
            /// <summary>
            /// Use ALB.(force HTTP/2 over TLS)
            /// </summary>
            Alb,
        }
    }

    /// <summary>
    /// FargateSpec follow to the rule.
    /// https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-cpu-memory-error.html
    /// </summary>
    public class Fargate
    {
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

        public Fargate() : this(CpuSpec.Quater, MemorySpec.Low)
        {
        }
        public Fargate(CpuSpec cpu, MemorySpec memory)
        {
            _cpuSize = (int)cpu;
            _memorysize = CalculateMemorySize(cpu, memory);
        }
        public Fargate(CpuSpec cpu, int memorySize)
        {
            _cpuSize = (int)cpu;
            _memorysize = CalculateMemorySize(cpu, memorySize);
        }

        /// <summary>
        /// Memory Calculation for Typical MemorySize
        /// </summary>
        /// <param name="cpu"></param>
        /// <param name="memory"></param>
        /// <returns></returns>
        private int CalculateMemorySize(CpuSpec cpu, MemorySpec memory) => (int)cpu * (int)memory;
        /// <summary>
        /// Memory Calculation for Custom MemorySize
        /// </summary>
        /// <param name="memorySize"></param>
        /// <returns></returns>
        private int CalculateMemorySize(CpuSpec cpu, int memorySize)
        {
            switch (cpu)
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

        /// <summary>
        /// Fargate Cpu Spec. default 256 = 0.25
        /// </summary>
        public enum CpuSpec
        {
            Quater = 256,
            Half = 512,
            Single = 1024,
            Double = 2048,
            Quadruple = 4096,
        }

        /// <summary>
        /// Fargate Memory Spec. default 512 = 0.5GB
        /// </summary>
        public enum MemorySpec
        {
            /// <summary>
            /// Only available when <see cref="CpuSpec"/> is Double or Quadruple.
            /// </summary>
            Custom = 0,
            Low = 2,
            Medium = 4,
            High = 8,
        }
    }
}
