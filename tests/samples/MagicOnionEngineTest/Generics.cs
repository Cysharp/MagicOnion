using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MessagePack;

namespace MagicOnionEngineTest;

public interface IMyGenericsService : IService<IMyGenericsService>
{
    UnaryResult<Nil> MethodA();
    UnaryResult<Nil> MethodB();
}

public interface IMyGenericsHub : IStreamingHub<IMyGenericsHub, IMyGenericsHubReceiver>
{
    Task MethodA();
    Task MethodB();
}

public interface IMyGenericsHubReceiver
{}


public class MyGenericsDefinitionService<T> : ServiceBase<IMyGenericsService>, IMyGenericsService
{
    public UnaryResult<Nil> MethodA() => default;
    public UnaryResult<Nil> MethodB() => default;
    public UnaryResult<Nil> MethodC() => default; // Non-Service method
    UnaryResult<Nil> MethodD() => default; // Non-Service method
}

public class MyGenericsDefinitionHub<T> : StreamingHubBase<IMyGenericsHub, IMyGenericsHubReceiver>, IMyGenericsHub
{
    public Task MethodA() => Task.CompletedTask;
    public Task MethodB() => Task.CompletedTask;
    public Task MethodC() => Task.CompletedTask; // Non-Service method
    Task MethodD() => Task.CompletedTask; // Non-Service method
}

public class MyConstructedGenericsService : MyGenericsDefinitionService<int> {}
public class MyConstructedGenericsHub : MyGenericsDefinitionHub<int> {}
