using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

public abstract class UnaryLargePayloadScenarioBase : IScenario
{
    IPerfTestService client = default!;
    readonly int payloadSize;

    public UnaryLargePayloadScenarioBase(int payloadSize)
    {
        this.payloadSize = payloadSize;
    }

    public ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = MagicOnionClient.Create<IPerfTestService>(channel);
        return ValueTask.CompletedTask;
    }

    public async ValueTask RunAsync(PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        var data = new byte[payloadSize];

        while (!cancellationToken.IsCancellationRequested)
        {
            _ = await client.UnaryLargePayloadAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011, data);
            ctx.Increment();
        }
    }
}

public class UnaryLargePayload1KScenario : UnaryLargePayloadScenarioBase
{
    public UnaryLargePayload1KScenario() : base(1024 * 1)
    {}
}

public class UnaryLargePayload2KScenario : UnaryLargePayloadScenarioBase
{
    public UnaryLargePayload2KScenario() : base(1024 * 2)
    {}
}

public class UnaryLargePayload4KScenario : UnaryLargePayloadScenarioBase
{
    public UnaryLargePayload4KScenario() : base(1024 * 4)
    {}
}

public class UnaryLargePayload8KScenario : UnaryLargePayloadScenarioBase
{
    public UnaryLargePayload8KScenario() : base(1024 * 8)
    {}
}

public class UnaryLargePayload16KScenario : UnaryLargePayloadScenarioBase
{
    public UnaryLargePayload16KScenario() : base(1024 * 16)
    {}
}

public class UnaryLargePayload32KScenario : UnaryLargePayloadScenarioBase
{
    public UnaryLargePayload32KScenario() : base(1024 * 32)
    {}
}

public class UnaryLargePayload64KScenario : UnaryLargePayloadScenarioBase
{
    public UnaryLargePayload64KScenario() : base(1024 * 64)
    {}
}
