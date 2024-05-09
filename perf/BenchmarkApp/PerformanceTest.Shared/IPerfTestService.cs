using MagicOnion;
using MessagePack;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime;
using MemoryPack;

namespace PerformanceTest.Shared;

public interface IPerfTestService : IService<IPerfTestService>
{
    UnaryResult<Nil> UnaryParameterless();
    UnaryResult<string> UnaryArgRefReturnRef(string arg1, int arg2, int arg3);
    UnaryResult<string> UnaryArgDynamicArgumentTupleReturnRef(string arg1, int arg2, int arg3, int arg4);
    UnaryResult<int> UnaryArgDynamicArgumentTupleReturnValue(string arg1, int arg2, int arg3, int arg4);
    UnaryResult<(int StatusCode, byte[] Data)> UnaryLargePayloadAsync(string arg1, int arg2, int arg3, int arg4, byte[] arg5);
    UnaryResult<ComplexResponse> UnaryComplexAsync(string arg1, int arg2, int arg3, int arg4);
}
