using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

public abstract class StreamingHubLargePayloadScenarioBase : IScenario, IPerfTestHubReceiver
{
    IPerfTestHub client = default!;
    readonly int payloadSize;

    public StreamingHubLargePayloadScenarioBase(int payloadSize)
    {
        this.payloadSize = payloadSize;
    }

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = await StreamingHubClient.ConnectAsync<IPerfTestHub, IPerfTestHubReceiver>(channel, this);
    }

    public async ValueTask RunAsync(PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        var data = new byte[payloadSize];

        while (!cancellationToken.IsCancellationRequested)
        {
            _ = await client.CallMethodLargePayloadAsync("Foo", 123, data);
            ctx.Increment();
        }
    }
}

public class StreamingHubLargePayload1KScenario : StreamingHubLargePayloadScenarioBase
{
    public StreamingHubLargePayload1KScenario() : base(1024 * 1)
    {}
}

public class StreamingHubLargePayload2KScenario : StreamingHubLargePayloadScenarioBase
{
    public StreamingHubLargePayload2KScenario() : base(1024 * 2)
    {}
}

public class StreamingHubLargePayload4KScenario : StreamingHubLargePayloadScenarioBase
{
    public StreamingHubLargePayload4KScenario() : base(1024 * 4)
    {}
}

public class StreamingHubLargePayload8KScenario : StreamingHubLargePayloadScenarioBase
{
    public StreamingHubLargePayload8KScenario() : base(1024 * 8)
    {}
}

public class StreamingHubLargePayload16KScenario : StreamingHubLargePayloadScenarioBase
{
    public StreamingHubLargePayload16KScenario() : base(1024 * 16)
    {}
}

public class StreamingHubLargePayload32KScenario : StreamingHubLargePayloadScenarioBase
{
    public StreamingHubLargePayload32KScenario() : base(1024 * 32)
    {}
}

public class StreamingHubLargePayload64KScenario : StreamingHubLargePayloadScenarioBase
{
    public StreamingHubLargePayload64KScenario() : base(1024 * 64)
    {}
}