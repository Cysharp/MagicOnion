using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class PerfTestService : ServiceBase<IPerfTestService>, IPerfTestService
{
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
        return UnaryResult((123, arg3));
    }

    public UnaryResult<ComplexResponse> UnaryComplexAsync(string arg1, int arg2)
    {
        return UnaryResult(ComplexResponse.Cached);
    }
}