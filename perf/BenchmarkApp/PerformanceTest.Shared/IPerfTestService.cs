using MagicOnion;
using MessagePack;

namespace PerformanceTest.Shared;

public interface IPerfTestService : IService<IPerfTestService>
{
    UnaryResult<Nil> UnaryParameterless();
    UnaryResult<string> UnaryArgRefReturnRef(string arg1);
    UnaryResult<string> UnaryArgDynamicArgumentTupleReturnRef(string arg1, int arg2);
    UnaryResult<int> UnaryArgDynamicArgumentTupleReturnValue(string arg1, int arg2);
}