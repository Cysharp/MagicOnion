using System.Collections.Concurrent;
using Grpc.Net.Client;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Integration.Tests;

public class StreamingHubClientResultTest(MagicOnionApplicationFactory<StreamingHubClientResultTestHub> factory) : IClassFixture<MagicOnionApplicationFactory<StreamingHubClientResultTestHub>>
{
    readonly MagicOnionApplicationFactory<StreamingHubClientResultTestHub> factory = factory;

    public static IEnumerable<object[]> EnumerateStreamingHubClientFactory()
    {
        //yield return new[] { new TestStreamingHubClientFactory("Dynamic", DynamicStreamingHubClientFactoryProvider.Instance) };
        yield return new[] { new TestStreamingHubClientFactory("Static", MagicOnionGeneratedClientInitializer.StreamingHubClientFactoryProvider) };
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Zero_NoResultValue(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Parameter_Zero_NoResultValue();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_Zero_NoResultValue), (FakeStreamingHubClientResultTestHubReceiver.ArgumentEmpty)), receiver.Received[0]);
        Assert.Equal((StreamingHubClientResultTestHub.Empty), serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Parameter_Zero_NoResultValue)));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Zero(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Parameter_Zero();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_Zero), (FakeStreamingHubClientResultTestHubReceiver.ArgumentEmpty)), receiver.Received[0]);
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_Zero)), serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Parameter_Zero)));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Many(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Parameter_Many();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_Many), ("Hello", 12345, true)), receiver.Received[0]);
        Assert.Equal($"{nameof(IStreamingHubClientResultTestHubReceiver.Parameter_Many)}:Hello,12345,True", serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Parameter_Many)));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Zero_With_Cancellation(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Parameter_Zero_With_Cancellation();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_Zero_With_Cancellation), (CancellationToken.None) /* Always None */), receiver.Received[0]);
        Assert.Equal($"System.Threading.Tasks.TaskCanceledException", serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Parameter_Zero_With_Cancellation)));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Zero_With_Cancellation_Optional(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Parameter_Zero_With_Cancellation_Optional();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_Zero_With_Cancellation_Optional), (CancellationToken.None) /* Always None */), receiver.Received[0]);
        Assert.Equal($"System.Threading.Tasks.TaskCanceledException", serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Parameter_Zero_With_Cancellation_Optional)));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Many_With_Cancellation(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Parameter_Many_With_Cancellation();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_Many_With_Cancellation), ("Hello", 12345, true, CancellationToken.None) /* Always None */), receiver.Received[0]);
        Assert.Equal($"System.Threading.Tasks.TaskCanceledException", serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Parameter_Many_With_Cancellation)));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Invoke_Parameter_Many_With_Cancellation_Optional(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Parameter_Many_With_Cancellation_Optional();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_Many_With_Cancellation_Optional), ("Hello", 12345, true, CancellationToken.None) /* Always None */), receiver.Received[0]);
        Assert.Equal($"System.Threading.Tasks.TaskCanceledException", serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Parameter_Many_With_Cancellation_Optional)));
    }

}

file class FakeStreamingHubClientResultTestHubReceiver : IStreamingHubClientResultTestHubReceiver
{
    public List<(string Method, object Arguments)> Received { get; } = new();

    public static readonly object ArgumentEmpty = new();

    public async Task Parameter_Zero_NoResultValue()
    {
        Received.Add((nameof(Parameter_Zero_NoResultValue), (ArgumentEmpty)));
        await Task.Delay(10);
    }

    public async Task<string> Parameter_Zero()
    {
        Received.Add((nameof(Parameter_Zero), (ArgumentEmpty)));
        await Task.Delay(10);
        return $"{nameof(Parameter_Zero)}";
    }

    public async Task<string> Parameter_Many(string arg1, int arg2, bool arg3)
    {
        Received.Add((nameof(Parameter_Many), (arg1, arg2, arg3)));
        await Task.Delay(10);
        return $"{nameof(Parameter_Many)}:{arg1},{arg2},{arg3}";
    }

    public Task<string> Throw()
    {
        Received.Add((nameof(Throw), (ArgumentEmpty)));
        throw new InvalidOperationException("Something went wrong.");
    }

    public async Task<string> Parameter_Zero_With_Cancellation(CancellationToken cancellationToken)
    {
        Received.Add((nameof(Parameter_Zero_With_Cancellation), (cancellationToken)));
        await Task.Delay(1000);
        return $"{nameof(Parameter_Zero_With_Cancellation)}:{cancellationToken.CanBeCanceled}";
    }

    public async Task<string> Parameter_Zero_With_Cancellation_Optional(CancellationToken cancellationToken = default)
    {
        Received.Add((nameof(Parameter_Zero_With_Cancellation_Optional), (cancellationToken)));
        await Task.Delay(1000);
        return $"{nameof(Parameter_Zero_With_Cancellation_Optional)}:{cancellationToken.CanBeCanceled}";
    }

    public async Task<string> Parameter_Many_With_Cancellation(string arg1, int arg2, bool arg3, CancellationToken cancellationToken)
    {
        Received.Add((nameof(Parameter_Many_With_Cancellation), (arg1, arg2, arg3, cancellationToken)));
        await Task.Delay(1000);
        return $"{nameof(Parameter_Many_With_Cancellation)}:{arg1},{arg2},{arg3},{cancellationToken.CanBeCanceled}";
    }

    public async Task<string> Parameter_Many_With_Cancellation_Optional(string arg1, int arg2, bool arg3, CancellationToken cancellationToken = default)
    {
        Received.Add((nameof(Parameter_Many_With_Cancellation_Optional), (arg1, arg2, arg3, cancellationToken)));
        await Task.Delay(1000);
        return $"{nameof(Parameter_Many_With_Cancellation_Optional)}:{arg1},{arg2},{arg3},{cancellationToken.CanBeCanceled}";
    }
}


public interface IStreamingHubClientResultTestHub : IStreamingHub<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>
{
    Task Invoke_Parameter_Zero_NoResultValue();
    Task Invoke_Parameter_Zero();
    Task Invoke_Parameter_Many();
    Task Invoke_Throw();
    Task Invoke_Parameter_Zero_With_Cancellation();
    Task Invoke_Parameter_Zero_With_Cancellation_Optional();
    Task Invoke_Parameter_Many_With_Cancellation();
    Task Invoke_Parameter_Many_With_Cancellation_Optional();
}

public interface IStreamingHubClientResultTestHubReceiver
{
    Task Parameter_Zero_NoResultValue();
    Task<string> Parameter_Zero();
    Task<string> Parameter_Many(string arg1, int arg2, bool arg3);
    Task<string> Throw();
    Task<string> Parameter_Zero_With_Cancellation(CancellationToken cancellationToken);
    Task<string> Parameter_Zero_With_Cancellation_Optional(CancellationToken cancellationToken = default);
    Task<string> Parameter_Many_With_Cancellation(string arg1, int arg2, bool arg3, CancellationToken cancellationToken);
    Task<string> Parameter_Many_With_Cancellation_Optional(string arg1, int arg2, bool arg3, CancellationToken cancellationToken = default);
}

public class StreamingHubClientResultTestHub([FromKeyedServices(MagicOnionApplicationFactory<StreamingHubClientResultTestHub>.ItemsKey)]ConcurrentDictionary<string, object> Items)
    : StreamingHubBase<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>, IStreamingHubClientResultTestHub
{
    public static readonly object Empty = new();

    public async Task Invoke_Parameter_Zero_NoResultValue()
    {
        await Client.Parameter_Zero_NoResultValue();
        Items.TryAdd(nameof(Invoke_Parameter_Zero_NoResultValue), Empty);
    }

    public async Task Invoke_Parameter_Zero()
    {
        var result = await Client.Parameter_Zero();
        Items.TryAdd(nameof(Invoke_Parameter_Zero), result);
    }

    public async Task Invoke_Parameter_Many()
    {
        var result = await Client.Parameter_Many("Hello", 12345, true);
        Items.TryAdd(nameof(Invoke_Parameter_Many), result);
    }

    public async Task Invoke_Throw()
    {
        try
        {
            var result = await Client.Throw();
        }
        catch (Exception e)
        {
           Items.TryAdd(nameof(Invoke_Throw), (e.GetType().FullName, e.Message));
        }
    }

    public async Task Invoke_Parameter_Zero_With_Cancellation()
    {
        var tcs = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_Zero_With_Cancellation(tcs.Token);
            Items.TryAdd(nameof(Invoke_Parameter_Zero_With_Cancellation), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_Zero_With_Cancellation), (e.GetType().FullName!));
        }
    }

    public async Task Invoke_Parameter_Zero_With_Cancellation_Optional()
    {
        var tcs = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_Zero_With_Cancellation_Optional(tcs.Token);
            Items.TryAdd(nameof(Invoke_Parameter_Zero_With_Cancellation), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_Zero_With_Cancellation_Optional), (e.GetType().FullName!));
        }
    }

    public async Task Invoke_Parameter_Many_With_Cancellation()
    {
        var tcs = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_Many_With_Cancellation("Hello", 12345, true, tcs.Token);
            Items.TryAdd(nameof(Invoke_Parameter_Many_With_Cancellation), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_Many_With_Cancellation), (e.GetType().FullName!));
        }
    }

    public async Task Invoke_Parameter_Many_With_Cancellation_Optional()
    {
        var tcs = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_Many_With_Cancellation_Optional("Hello", 12345, true, tcs.Token);
            Items.TryAdd(nameof(Invoke_Parameter_Many_With_Cancellation_Optional), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_Many_With_Cancellation_Optional), (e.GetType().FullName!));
        }
    }
}
