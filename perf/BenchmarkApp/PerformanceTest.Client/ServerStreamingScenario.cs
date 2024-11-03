using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using PerformanceTest.Shared;

namespace PerformanceTest.Client;

public class ServerStreamingScenario : IScenario
{
    IPerfTestService client = default!;
    ServerStreamingResult<SimpleResponse> stream = default!;
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
        this.stream = await client.ServerStreamingAsync(ctx.Timeout);
        while (!cancellationToken.IsCancellationRequested)
        {
            ctx.Increment();
            try
            {
                var begin = timeProvider.GetTimestamp();
                await stream.ResponseStream.MoveNext(cancellationToken);
                ctx.LatencyThrottled(connectionId, timeProvider.GetElapsedTime(begin), 100); // avoid OOM
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
            catch (Exception)
            {
                ctx.Error();
                break;
            }
        }
    }

    public async Task CompleteAsync()
    {
        try
        {
            // wait for server complete
            await foreach (var _ in stream.ResponseStream.ReadAllAsync()) { };
        }
        catch (Exception)
        {
            // do nothing.
        }
        stream.Dispose();
    }
}
