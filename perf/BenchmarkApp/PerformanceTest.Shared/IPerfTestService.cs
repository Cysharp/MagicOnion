using MagicOnion;
using MessagePack;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime;
using MemoryPack;

namespace PerformanceTest.Shared;

public interface IPerfTestService : IService<IPerfTestService>
{
    UnaryResult<ServerInformation> GetServerInformationAsync();
    UnaryResult<Nil> UnaryParameterless();
    UnaryResult<string> UnaryArgRefReturnRef(string arg1, int arg2, int arg3);
    UnaryResult<string> UnaryArgDynamicArgumentTupleReturnRef(string arg1, int arg2, int arg3, int arg4);
    UnaryResult<int> UnaryArgDynamicArgumentTupleReturnValue(string arg1, int arg2, int arg3, int arg4);
    UnaryResult<(int StatusCode, byte[] Data)> UnaryLargePayloadAsync(string arg1, int arg2, int arg3, int arg4, byte[] arg5);
    UnaryResult<ComplexResponse> UnaryComplexAsync(string arg1, int arg2, int arg3, int arg4);
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
