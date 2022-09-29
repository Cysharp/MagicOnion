using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;

namespace MagicOnionEngineTest;

public interface IMyService : IService<IMyService>
{
    UnaryResult<Nil> MethodA();
    UnaryResult<Nil> MethodB();
}

public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
    Task MethodA();
    Task MethodB();
}

public interface IMyHubReceiver
{}


public class MyService : ServiceBase<IMyService>, IMyService
{
    public UnaryResult<Nil> MethodA() => default;
    public UnaryResult<Nil> MethodB() => default;
    public UnaryResult<Nil> MethodC() => default; // Non-Service method
    UnaryResult<Nil> MethodD() => default; // Non-Service method
}

public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>, IMyHub
{
    public Task MethodA() => Task.CompletedTask;
    public Task MethodB() => Task.CompletedTask;
    public Task MethodC() => Task.CompletedTask; // Non-Service method
    Task MethodD() => Task.CompletedTask; // Non-Service method
}
