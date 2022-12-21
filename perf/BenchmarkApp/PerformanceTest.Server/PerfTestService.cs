using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestService : ServiceBase<IPerfTestService>, IPerfTestService
{
    public UnaryResult<ServerInformation> GetServerInformationAsync()
    {
        return UnaryResult.FromResult(new ServerInformation(
            Environment.MachineName,
            ApplicationInformation.Current.MagicOnionVersion,
            ApplicationInformation.Current.GrpcNetVersion,
            ApplicationInformation.Current.MessagePackVersion,
            ApplicationInformation.Current.IsReleaseBuild,
            ApplicationInformation.Current.FrameworkDescription,
            ApplicationInformation.Current.OSDescription,
            ApplicationInformation.Current.OSArchitecture,
            ApplicationInformation.Current.ProcessArchitecture,
            ApplicationInformation.Current.IsServerGC,
            ApplicationInformation.Current.ProcessorCount,
            ApplicationInformation.Current.IsAttached));
    }

    public UnaryResult<Nil> UnaryParameterless()
    {
        return new UnaryResult<Nil>(Nil.Default);
    }

    public UnaryResult<string> UnaryArgRefReturnRef(string arg1)
    {
        return new UnaryResult<string>(arg1);
    }

    public UnaryResult<string> UnaryArgDynamicArgumentTupleReturnRef(string arg1, int arg2)
    {
        return new UnaryResult<string>(arg1 + arg2);
    }

    public UnaryResult<int> UnaryArgDynamicArgumentTupleReturnValue(string arg1, int arg2)
    {
        return new UnaryResult<int>(arg2);
    }

    public UnaryResult<(int StatusCode, byte[] Data)> UnaryLargePayloadAsync(string arg1, int arg2, byte[] arg3)
    {
        return UnaryResult.FromResult((123, arg3));
    }

    public UnaryResult<ComplexResponse> UnaryComplexAsync(string arg1, int arg2)
    {
        return UnaryResult.FromResult(ComplexResponse.Cached);
    }
}
