using JetBrains.Profiler.Api;
using MagicOnion;
using MagicOnion.Server;
using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;

namespace PerformanceTest.Server;

public class PerfTestControlService : ServiceBase<IPerfTestControlService>, IPerfTestControlService
{
    public UnaryResult<ServerInformation> GetServerInformationAsync()
    {
        return UnaryResult.FromResult(new ServerInformation(
            Environment.MachineName,
            ApplicationInformation.Current.BenchmarkerVersion,
            ApplicationInformation.Current.MagicOnionVersion,
            ApplicationInformation.Current.GrpcNetVersion,
            ApplicationInformation.Current.MessagePackVersion,
            ApplicationInformation.Current.MemoryPackVersion,
            ApplicationInformation.Current.IsReleaseBuild,
            ApplicationInformation.Current.FrameworkDescription,
            ApplicationInformation.Current.OSDescription,
            ApplicationInformation.Current.OSArchitecture,
            ApplicationInformation.Current.ProcessArchitecture,
            ApplicationInformation.Current.CpuModelName,
            ApplicationInformation.Current.IsServerGC,
            ApplicationInformation.Current.ProcessorCount,
            ApplicationInformation.Current.IsAttached));
    }

    public UnaryResult<string> ExchangeMagicOnionVersionTagAsync(string? clientMagicOnionVersion)
    {
        var versionTag = $"{clientMagicOnionVersion}x{ApplicationInformation.Current.TagMagicOnionVersion}";

        // keep in server
        DatadogMetricsRecorder.MagicOnionVersions = versionTag;
        return UnaryResult.FromResult(versionTag);
    }

    public UnaryResult SetMemoryProfilerCollectAllocationsAsync(bool enable)
    {
        MemoryProfiler.CollectAllocations(enable);
        return UnaryResult.CompletedResult;
    }

    public UnaryResult CreateMemoryProfilerSnapshotAsync(string name)
    {
        MemoryProfiler.GetSnapshot(name);
        return UnaryResult.CompletedResult;
    }
}
