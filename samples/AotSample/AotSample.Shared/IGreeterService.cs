using MagicOnion;

namespace AotSample.Shared;

public interface IGreeterService : IService<IGreeterService>
{
    UnaryResult<string> SayHelloAsync(string name);
    UnaryResult<int> AddAsync(int a, int b);
}
