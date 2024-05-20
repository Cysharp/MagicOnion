using JetBrains.Profiler.Api;
using MagicOnion;
using MagicOnion.Server;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestControlService : ServiceBase<IPerfTestControlService>, IPerfTestControlService
{
    public UnaryResult<ServerInformation> GetServerInformationAsync()
    {
        return UnaryResult.FromResult(new ServerInformation(
            Environment.MachineName,
            ApplicationInformation.Current.MagicOnionVersion,
            ApplicationInformation.Current.GrpcNetVersion,
            ApplicationInformation.Current.MessagePackVersion,
            ApplicationInformation.Current.MemoryPackVersion,
            ApplicationInformation.Current.IsReleaseBuild,
            ApplicationInformation.Current.FrameworkDescription,
            ApplicationInformation.Current.OSDescription,
            ApplicationInformation.Current.OSArchitecture,
            ApplicationInformation.Current.ProcessArchitecture,
            ApplicationInformation.Current.IsServerGC,
            ApplicationInformation.Current.ProcessorCount,
            ApplicationInformation.Current.IsAttached));
    }

    public UnaryResult SetMemoryProfilerCollectAllocations(bool enable)
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
