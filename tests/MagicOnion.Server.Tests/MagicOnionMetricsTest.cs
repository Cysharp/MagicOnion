using System.Diagnostics.Metrics;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization.MessagePack;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;

namespace MagicOnion.Server.Tests;

public class MagicOnionMetricsTest : IClassFixture<MagicOnionApplicationFactory<MagicOnionMetricsTestHub>>
{
    readonly WebApplicationFactory<Program> factory;
    readonly TestMeterFactory meterFactory;

    public MagicOnionMetricsTest(MagicOnionApplicationFactory<MagicOnionMetricsTestHub> fixture)
    {
        meterFactory = new TestMeterFactory();
        factory = fixture.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<MagicOnionMetrics>(new MagicOnionMetrics(meterFactory));
            });
        });
    }

    [Fact]
    public async Task StreamingHubConnectionCounter()
    {
        var receiver = new Receiver();
        using var collector = new MetricCollector<long>(meterFactory, MagicOnionMetrics.MeterName, "magiconion.server.streaminghub.connections");
        IReadOnlyList<CollectedMeasurement<long>> values = collector.GetMeasurementSnapshot();
        IReadOnlyList<CollectedMeasurement<long>> values2;
        IReadOnlyList<CollectedMeasurement<long>> values3;

        {
            using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
            var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default, cancellationToken: TestContext.Current.CancellationToken);
            values2 = collector.GetMeasurementSnapshot();
            await client.DisposeAsync();
        }

        values3 = collector.GetMeasurementSnapshot();

        Assert.Empty(values);

        Assert.Single(values2);
        Assert.Equal(1, values2[0].Value);
        Assert.Equal("magiconion", values2[0].Tags["rpc.system"]);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub), values2[0].Tags["rpc.service"]);

        Assert.Equal(2, values3.Count());
        Assert.Equal(1, values3[0].Value);
        Assert.Equal(-1, values3[1].Value);
        Assert.Equal("magiconion", values3[1].Tags["rpc.system"]);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub), values3[1].Tags["rpc.service"]);
    }

    [Fact]
    public async Task StreamingHubMethodDuration()
    {
        var receiver = new Receiver();
        using var collector = new MetricCollector<long>(meterFactory, MagicOnionMetrics.MeterName, "magiconion.server.streaminghub.method_duration");
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default, cancellationToken: TestContext.Current.CancellationToken);

        await client.SleepAsync();
        await Task.Delay(100, TestContext.Current.CancellationToken);

        var values = collector.GetMeasurementSnapshot();

        Assert.Single(values);
        Assert.True(values[0].Value >= 90);
        Assert.Equal("magiconion", values[0].Tags["rpc.system"]);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub), values[0].Tags["rpc.service"]);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub.SleepAsync), values[0].Tags["rpc.method"]);
    }

    [Fact]
    public async Task StreamingHubMethodCompleted()
    {
        var receiver = new Receiver();
        using var collector = new MetricCollector<long>(meterFactory, MagicOnionMetrics.MeterName, "magiconion.server.streaminghub.method_completed");
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default, cancellationToken: TestContext.Current.CancellationToken);

        await client.MethodAsync();
        await Task.Delay(100, TestContext.Current.CancellationToken);

        var values = collector.GetMeasurementSnapshot();

        Assert.Single(values);
        Assert.Equal(1, values[0].Value);
        Assert.Equal("magiconion", values[0].Tags["rpc.system"]);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub), values[0].Tags["rpc.service"]);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub.MethodAsync), values[0].Tags["rpc.method"]);
        Assert.Equal(false, values[0].Tags["magiconion.streaminghub.is_error"]);
    }

    [Fact]
    public async Task StreamingHubMethodCompleted_Failure()
    {
        var receiver = new Receiver();
        using var collector = new MetricCollector<long>(meterFactory, MagicOnionMetrics.MeterName, "magiconion.server.streaminghub.method_completed");
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default, cancellationToken: TestContext.Current.CancellationToken);

        try
        {
            await client.ThrowAsync();
        }
        catch
        { }
        await Task.Delay(100, TestContext.Current.CancellationToken);

        var values = collector.GetMeasurementSnapshot();

        Assert.Single(values);
        Assert.Equal(1, values[0].Value);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub), values[0].Tags["rpc.service"]);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub.ThrowAsync), values[0].Tags["rpc.method"]);
        Assert.Equal(true, values[0].Tags["magiconion.streaminghub.is_error"]);
    }

    [Fact]
    public async Task StreamingHubException()
    {
        var receiver = new Receiver();
        using var collector = new MetricCollector<long>(meterFactory, MagicOnionMetrics.MeterName, "magiconion.server.streaminghub.exceptions");
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default, cancellationToken: TestContext.Current.CancellationToken);

        try
        {
            await client.ThrowAsync();
        }
        catch
        { }
        await Task.Delay(100, TestContext.Current.CancellationToken);

        var values = collector.GetMeasurementSnapshot();

        Assert.Single(values);
        Assert.Equal(1, values[0].Value);
        Assert.Equal("magiconion", values[0].Tags["rpc.system"]);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub), values[0].Tags["rpc.service"]);
        Assert.Equal(nameof(IMagicOnionMetricsTestHub.ThrowAsync), values[0].Tags["rpc.method"]);
        Assert.Equal("System.InvalidOperationException", values[0].Tags["error.type"]);
    }

    class Receiver : IMagicOnionMetricsTestHubReceiver
    {}
}

public class MagicOnionMetricsTestHub : StreamingHubBase<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>, IMagicOnionMetricsTestHub
{
    public Task MethodAsync()
        => Task.CompletedTask;
    public async Task SleepAsync()
        => await Task.Delay(100);
    public Task ThrowAsync()
        => throw new InvalidOperationException();
}

public interface IMagicOnionMetricsTestHub : IStreamingHub<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>
{
    Task MethodAsync();
    Task SleepAsync();
    Task ThrowAsync();
}

public interface IMagicOnionMetricsTestHubReceiver
{ }

class TestMeterFactory : IMeterFactory
{
    public List<Meter> Meters { get; } = new List<Meter>();

    public void Dispose()
    {
        foreach (var meter in Meters)
        {
            meter.Dispose();
        }
        Meters.Clear();
    }

    public Meter Create(MeterOptions options)
    {
        var meter = new Meter(options.Name, options.Version, options.Tags, scope: this);
        Meters.Add(meter);
        return meter;
    }
}
