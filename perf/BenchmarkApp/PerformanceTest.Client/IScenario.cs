using Grpc.Net.Client;

public interface IScenario
{
    ValueTask PrepareAsync(GrpcChannel channel);
    ValueTask RunAsync(PerformanceTestRunningContext ctx, CancellationToken cancellationToken);
}