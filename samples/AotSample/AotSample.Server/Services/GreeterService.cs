using AotSample.Shared;
using MagicOnion;
using MagicOnion.Server;

namespace AotSample.Server.Services;

public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public UnaryResult<string> SayHelloAsync(string name)
    {
        return UnaryResult.FromResult($"Hello, {name}!");
    }

    public UnaryResult<int> AddAsync(int a, int b)
    {
        return UnaryResult.FromResult(a + b);
    }
}
