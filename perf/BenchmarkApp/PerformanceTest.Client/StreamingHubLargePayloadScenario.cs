using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

public abstract class StreamingHubLargePayloadScenarioBase : IScenario, IPerfTestHubReceiver
{
    IPerfTestHub client = default!;
    readonly int payloadSize;
    readonly TimeProvider timeProvider = TimeProvider.System;

    public StreamingHubLargePayloadScenarioBase(int payloadSize)
    {
        this.payloadSize = payloadSize;
    }

    public async ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = await StreamingHubClient.ConnectAsync<IPerfTestHub, IPerfTestHubReceiver>(channel, this);
    }

    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        var data = new byte[payloadSize];

        while (!cancellationToken.IsCancellationRequested)
        {
            var begin = timeProvider.GetTimestamp();
            _ = await client.CallMethodLargePayloadAsync("FooBarBaz🚀こんにちは世界", 123, 4567, 891011, data);
            ctx.Increment();
            ctx.Latency(connectionId, timeProvider.GetElapsedTime(begin));
        }
    }

    public async Task CompleteAsync()
    {
        await this.client.DisposeAsync();
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
