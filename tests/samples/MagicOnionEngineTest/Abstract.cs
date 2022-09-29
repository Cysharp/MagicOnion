using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;

namespace MagicOnionEngineTest;

public interface IMyAbstractService : IService<IMyAbstractService>
{
    UnaryResult<Nil> MethodA();
    UnaryResult<Nil> MethodB();
}

public interface IMyAbstractHub : IStreamingHub<IMyAbstractHub, IMyAbstractHubReceiver>
{
    Task MethodA();
    Task MethodB();
}

public interface IMyAbstractHubReceiver
{}

public abstract class MyAbstractService : ServiceBase<IMyAbstractService>, IMyAbstractService
{
    public UnaryResult<Nil> MethodA() => default;
    public UnaryResult<Nil> MethodB() => default;
    public UnaryResult<Nil> MethodC() => default; // Non-Service method
}

public abstract class MyAbstractHub : StreamingHubBase<IMyAbstractHub, IMyAbstractHubReceiver>, IMyAbstractHub
{
    public Task MethodA() => Task.CompletedTask;
    public Task MethodB() => Task.CompletedTask;
}
