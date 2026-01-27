using MagicOnion;
using MessagePack;

namespace PerformanceTest.Shared;

public interface IPerfTestService : IService<IPerfTestService>
{
    // Unary
    UnaryResult<Nil> UnaryParameterless();
    UnaryResult<string> UnaryArgRefReturnRef(string arg1, int arg2, int arg3);
    UnaryResult<string> UnaryArgDynamicArgumentTupleReturnRef(string arg1, int arg2, int arg3, int arg4);
    UnaryResult<int> UnaryArgDynamicArgumentTupleReturnValue(string arg1, int arg2, int arg3, int arg4);
    UnaryResult<(int StatusCode, byte[] Data)> UnaryLargePayloadAsync(string arg1, int arg2, int arg3, int arg4, byte[] arg5);
    UnaryResult<ComplexResponse> UnaryComplexAsync(string arg1, int arg2, int arg3, int arg4);

    // ServerStreaming
    Task<ServerStreamingResult<SimpleResponse>> ServerStreamingAsync(TimeSpan timeout);
    // Broadcast
    UnaryResult<SimpleResponse> BroadcastAsync(TimeSpan timeout);
}
