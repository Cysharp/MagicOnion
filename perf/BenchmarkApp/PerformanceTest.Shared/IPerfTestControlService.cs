using MagicOnion;
using MemoryPack;
using MessagePack;
using System.Runtime.InteropServices;

namespace PerformanceTest.Shared;

public interface IPerfTestControlService : IService<IPerfTestControlService>
{
    UnaryResult<ServerInformation> GetServerInformationAsync();

    UnaryResult SetMemoryProfilerCollectAllocations(bool enable);
    UnaryResult CreateMemoryProfilerSnapshotAsync(string name);
}

[MessagePackObject(true)]
[MemoryPackable]
public partial class ServerInformation
{
    public string MachineName { get; set; }
    public string? MagicOnionVersion { get; }
    public string? GrpcNetVersion { get; }
    public string? MessagePackVersion { get; }
    public string? MemoryPackVersion { get; }
    public bool IsReleaseBuild { get; }
    public string FrameworkDescription { get; }
    public string OSDescription { get; }
    public Architecture OSArchitecture { get; }
    public Architecture ProcessArchitecture { get; }
    public bool IsServerGC { get; }
    public int ProcessorCount { get; }
    public bool IsAttached { get; }

    public ServerInformation(string machineName, string? magicOnionVersion, string? grpcNetVersion, string? messagePackVersion, string? memoryPackVersion, bool isReleaseBuild, string frameworkDescription, string osDescription, Architecture osArchitecture, Architecture processArchitecture, bool isServerGC, int processorCount, bool isAttached)
    {
        MachineName = machineName;
        MagicOnionVersion = magicOnionVersion;
        GrpcNetVersion = grpcNetVersion;
        MessagePackVersion = messagePackVersion;
        MemoryPackVersion = memoryPackVersion;
        IsReleaseBuild = isReleaseBuild;
        FrameworkDescription = frameworkDescription;
        OSDescription = osDescription;
        OSArchitecture = osArchitecture;
        ProcessArchitecture = processArchitecture;
        IsServerGC = isServerGC;
        ProcessorCount = processorCount;
        IsAttached = isAttached;
    }
}
