using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

public class UnaryScenario : IScenario
{
    IPerfTestService client = default!;

    public ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = MagicOnionClient.Create<IPerfTestService>(channel);
        return ValueTask.CompletedTask;
    }

    public async ValueTask RunAsync(PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await client.UnaryArgDynamicArgumentTupleReturnValue("Foo", 123);
            ctx.Increment();
        }
    }
}

public class UnaryComplexScenario : IScenario
{
    IPerfTestService client = default!;

    public ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = MagicOnionClient.Create<IPerfTestService>(channel);
        return ValueTask.CompletedTask;
    }

    public async ValueTask RunAsync(PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await client.UnaryComplexAsync("Foo", 1234567);
            ctx.Increment();
        }
    }
}