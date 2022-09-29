using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;

namespace MagicOnionEngineTest;

public interface IMyNonPublicService : IService<IMyNonPublicService>
{
    UnaryResult<Nil> MethodA();
    UnaryResult<Nil> MethodB();
}

public interface IMyNonPublicHub : IStreamingHub<IMyNonPublicHub, IMyNonPublicHubReceiver>
{
    Task MethodA();
    Task MethodB();
}

public interface IMyNonPublicHubReceiver
{}


internal class MyNonPublicService : ServiceBase<IMyNonPublicService>, IMyNonPublicService
{
    public UnaryResult<Nil> MethodA() => default;
    public UnaryResult<Nil> MethodB() => default;
    public UnaryResult<Nil> MethodC() => default; // Non-Service method
    UnaryResult<Nil> MethodD() => default; // Non-Service method
}

internal class MyNonPublicHub : StreamingHubBase<IMyNonPublicHub, IMyNonPublicHubReceiver>, IMyNonPublicHub
{
    public Task MethodA() => Task.CompletedTask;
    public Task MethodB() => Task.CompletedTask;
    public Task MethodC() => Task.CompletedTask; // Non-Service method
    Task MethodD() => Task.CompletedTask; // Non-Service method
}
