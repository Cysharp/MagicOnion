using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using PerformanceTest.Shared;

namespace PerformanceTest.Client;

public class ServerStreamingScenario : IScenario
{
    IPerfTestService client = default!;
    readonly TimeProvider timeProvider = TimeProvider.System;

    public ValueTask PrepareAsync(GrpcChannel channel)
    {
        this.client = MagicOnionClient.Create<IPerfTestService>(channel);
        return ValueTask.CompletedTask;
    }

    // Follow what the grpc-dotnet benchmark does, but this ServerStreaming benchmark seems meaningless as MoveNext may concatenate multiple responses from the server.
    // So most times MoveNext won't wait at all, and it may wait occasionally.
    public async ValueTask RunAsync(int connectionId, PerformanceTestRunningContext ctx, CancellationToken cancellationToken)
    {
        using var stream = await client.ServerStreamingAsync();
        while (!cancellationToken.IsCancellationRequested)
        {
            ctx.Increment();
            try
            {
                var begin = timeProvider.GetTimestamp();
                await stream.ResponseStream.MoveNext(cancellationToken);
                ctx.Latency(connectionId, timeProvider.GetElapsedTime(begin));
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled && cancellationToken.IsCancellationRequested)
            {
                // canceling call is expected behavior.
                break;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable && ex.InnerException is IOException)
            {
                // canceling call is expected behavior.
                break;
            }
        }
    }

    public Task CompleteAsync()
    {
        return Task.CompletedTask;
    }
}
