using JetBrains.Profiler.Api;
using MagicOnion;
using MagicOnion.Server;
using PerformanceTest.Shared;
using PerformanceTest.Shared.Reporting;

namespace PerformanceTest.Server;

public class PerfTestControlService(DatadogMetricsRecorder datadog, HardwarePerformanceReporter hardware) : ServiceBase<IPerfTestControlService>, IPerfTestControlService
{
    public UnaryResult<ServerInformation> GetServerInformationAsync()
    {
        return UnaryResult.FromResult(new ServerInformation(
            Environment.MachineName,
            ApplicationInformation.Current.BenchmarkerVersion,
            ApplicationInformation.Current.IsLatestMagicOnion,
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

    public UnaryResult<(string serverMagicOnionVersion, bool enableLatestTag)> ExchangeMagicOnionVersionTagAsync(string? clientMagicOnionVersion, bool isLatestMagicOnionVersion)
    {
        var versionTag = $"{clientMagicOnionVersion}x{ApplicationInformation.Current.TagMagicOnionVersion}";
        // Both Server and Client is latest
        var isLatestTagEnabled = ApplicationInformation.Current.IsLatestMagicOnion && isLatestMagicOnionVersion;

        // keep in server
        DatadogMetricsRecorder.MagicOnionVersions = versionTag;
        DatadogMetricsRecorder.EnableLatestTag = isLatestTagEnabled;
        return UnaryResult.FromResult((versionTag, isLatestTagEnabled));
    }

    public UnaryResult<string> ExchangeScenarioAsync(string scenario)
    {
        // keep in server
        DatadogMetricsRecorder.Scenario = scenario;
        return UnaryResult.FromResult(scenario);
    }

    public async UnaryResult NotifyCompleteScenarioAsync()
    {
        // flush hardware performance when scenario complete
        var result = hardware.GetResultAndClear();
        await datadog.PutServerHardwareMetricsAsync(ApplicationInformation.Current, result);
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
