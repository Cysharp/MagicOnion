using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;

namespace MagicOnionEngineTest;

public interface IMyIgnoredService : IService<IMyIgnoredService>
{
    UnaryResult<Nil> MethodA();
    UnaryResult<Nil> MethodB();
}

public interface IMyIgnoredHub : IStreamingHub<IMyIgnoredHub, IMyIgnoredHubReceiver>
{
    Task MethodA();
    Task MethodB();
}

public interface IMyIgnoredHubReceiver
{}

[Ignore]
public class MyIgnoredService : ServiceBase<IMyIgnoredService>, IMyIgnoredService
{
    public UnaryResult<Nil> MethodA() => default;
    public UnaryResult<Nil> MethodB() => default;
    public UnaryResult<Nil> MethodC() => default; // Non-Service method
}

[Ignore]
public class MyIgnoredHub : StreamingHubBase<IMyIgnoredHub, IMyIgnoredHubReceiver>, IMyIgnoredHub
{
    public Task MethodA() => Task.CompletedTask;
    public Task MethodB() => Task.CompletedTask;
}
