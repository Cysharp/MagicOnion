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
    readonly WebApplicationFactory<MagicOnionTestServer.Program> factory;
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
            var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default);
            values2 = collector.GetMeasurementSnapshot();
            await client.DisposeAsync();
        }

        values3 = collector.GetMeasurementSnapshot();

        values.Should().BeEmpty();

        values2.Should().HaveCount(1);
        values2[0].Value.Should().Be(1);
        values2[0].Tags["rpc.system"].Should().Be("magiconion");
        values2[0].Tags["rpc.service"].Should().Be(nameof(IMagicOnionMetricsTestHub));

        values3.Should().HaveCount(2);
        values3[0].Value.Should().Be(1);
        values3[1].Value.Should().Be(-1);
        values3[1].Tags["rpc.system"].Should().Be("magiconion");
        values3[1].Tags["rpc.service"].Should().Be(nameof(IMagicOnionMetricsTestHub));
    }

    [Fact]
    public async Task StreamingHubMethodDuration()
    {
        var receiver = new Receiver();
        using var collector = new MetricCollector<long>(meterFactory, MagicOnionMetrics.MeterName, "magiconion.server.streaminghub.method_duration");
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default);

        await client.SleepAsync();
        await Task.Delay(100);

        var values = collector.GetMeasurementSnapshot();

        values.Should().HaveCount(1);
        values[0].Value.Should().BeGreaterThanOrEqualTo(90);
        values[0].Tags["rpc.system"].Should().Be("magiconion");
        values[0].Tags["rpc.service"].Should().Be(nameof(IMagicOnionMetricsTestHub));
        values[0].Tags["rpc.method"].Should().Be(nameof(IMagicOnionMetricsTestHub.SleepAsync));
    }

    [Fact]
    public async Task StreamingHubMethodCompleted()
    {
        var receiver = new Receiver();
        using var collector = new MetricCollector<long>(meterFactory, MagicOnionMetrics.MeterName, "magiconion.server.streaminghub.method_completed");
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default);

        await client.MethodAsync();
        await Task.Delay(100);

        var values = collector.GetMeasurementSnapshot();

        values.Should().HaveCount(1);
        values[0].Value.Should().Be(1);
        values[0].Tags["rpc.system"].Should().Be("magiconion");
        values[0].Tags["rpc.service"].Should().Be(nameof(IMagicOnionMetricsTestHub));
        values[0].Tags["rpc.method"].Should().Be(nameof(IMagicOnionMetricsTestHub.MethodAsync));
        values[0].Tags["magiconion.streaminghub.is_error"].Should().Be(false);
    }

    [Fact]
    public async Task StreamingHubMethodCompleted_Failure()
    {
        var receiver = new Receiver();
        using var collector = new MetricCollector<long>(meterFactory, MagicOnionMetrics.MeterName, "magiconion.server.streaminghub.method_completed");
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default);

        try
        {
            await client.ThrowAsync();
        }
        catch
        { }
        await Task.Delay(100);

        var values = collector.GetMeasurementSnapshot();

        values.Should().HaveCount(1);
        values[0].Value.Should().Be(1);
        values[0].Tags["rpc.service"].Should().Be(nameof(IMagicOnionMetricsTestHub));
        values[0].Tags["rpc.method"].Should().Be(nameof(IMagicOnionMetricsTestHub.ThrowAsync));
        values[0].Tags["magiconion.streaminghub.is_error"].Should().Be(true);
    }

    [Fact]
    public async Task StreamingHubException()
    {
        var receiver = new Receiver();
        using var collector = new MetricCollector<long>(meterFactory, MagicOnionMetrics.MeterName, "magiconion.server.streaminghub.exceptions");
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = await StreamingHubClient.ConnectAsync<IMagicOnionMetricsTestHub, IMagicOnionMetricsTestHubReceiver>(channel, receiver, serializerProvider: MessagePackMagicOnionSerializerProvider.Default);

        try
        {
            await client.ThrowAsync();
        }
        catch
        { }
        await Task.Delay(100);

        var values = collector.GetMeasurementSnapshot();

        values.Should().HaveCount(1);
        values[0].Value.Should().Be(1);
        values[0].Tags["rpc.system"].Should().Be("magiconion");
        values[0].Tags["rpc.service"].Should().Be(nameof(IMagicOnionMetricsTestHub));
        values[0].Tags["rpc.method"].Should().Be(nameof(IMagicOnionMetricsTestHub.ThrowAsync));
        values[0].Tags["error.type"].Should().Be("System.InvalidOperationException");
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
