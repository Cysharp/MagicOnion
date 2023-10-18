using MagicOnion;

namespace SampleServiceDefinitions.Services;

public interface IGreeterService : IService<IGreeterService>
{
    UnaryResult<string> HelloAsync(string name, int age);
    UnaryResult PingAsync();
    UnaryResult<bool> CanGreetAsync();
}
