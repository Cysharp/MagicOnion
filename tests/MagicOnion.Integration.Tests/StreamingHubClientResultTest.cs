using System.Collections.Concurrent;
using Grpc.Net.Client;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace MagicOnion.Integration.Tests;

public class StreamingHubClientResultTest(MagicOnionApplicationFactory<StreamingHubClientResultTestHub> factory) : IClassFixture<MagicOnionApplicationFactory<StreamingHubClientResultTestHub>>
{
    readonly MagicOnionApplicationFactory<StreamingHubClientResultTestHub> factory = factory;

    public static IEnumerable<object[]> EnumerateStreamingHubClientFactory()
    {
        yield return new[] { new TestStreamingHubClientFactory("Dynamic", DynamicStreamingHubClientFactoryProvider.Instance) };
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
    public async Task Parameter_One_With_Cancellation(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Parameter_One_With_Cancellation();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_One_With_Cancellation), ("Hello", CancellationToken.None) /* Always None */), receiver.Received[0]);
        Assert.Equal($"System.Threading.Tasks.TaskCanceledException", serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Parameter_One_With_Cancellation)));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_One_With_Cancellation_Optional(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Parameter_One_With_Cancellation_Optional();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Parameter_One_With_Cancellation_Optional), ("Hello", CancellationToken.None) /* Always None */), receiver.Received[0]);
        Assert.Equal($"System.Threading.Tasks.TaskCanceledException", serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Parameter_One_With_Cancellation_Optional)));
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
    public async Task Parameter_Many_With_Cancellation_Optional(TestStreamingHubClientFactory clientFactory)
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

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Throw(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Throw();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Throw), (FakeStreamingHubClientResultTestHubReceiver.ArgumentEmpty)), receiver.Received[0]);
        Assert.Equal("Grpc.Core.RpcException", (((string,string))serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Throw))!).Item1);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Throw_With_StatusCode(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Throw_With_StatusCode();

        // Assert
        Assert.Equal((nameof(IStreamingHubClientResultTestHubReceiver.Throw_With_StatusCode), (FakeStreamingHubClientResultTestHubReceiver.ArgumentEmpty)), receiver.Received[0]);
        Assert.Equal("Grpc.Core.RpcException", (((string, string))serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Throw_With_StatusCode))!).Item1);
        Assert.Equal(StatusCode.Unauthenticated, ((StatusCode)serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Throw_With_StatusCode) + "/StatusCode")!));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Invoke_After_Disconnected(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;
        var signalFromClient = new SemaphoreSlim(0);
        var signalToClient = new SemaphoreSlim(0);
        serverItems[nameof(Invoke_After_Disconnected) + "/Signal/FromClient"] = signalFromClient;
        serverItems[nameof(Invoke_After_Disconnected) + "/Signal/ToClient"] = signalToClient;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        _ = client.Invoke_After_Disconnected();
        await Task.Delay(200);
        await client.DisposeAsync();
        channel.Dispose();
        signalFromClient.Release();

        // Wait for complete processing the request on the server.
        await signalToClient.WaitAsync();

        // Assert
        //testOutputHelper.WriteLine(serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_After_Disconnected) + "/Exception") + "");
        Assert.Empty(receiver.Received);
        Assert.Equal("System.Threading.Tasks.TaskCanceledException", (((string, string))serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_After_Disconnected))!).Item1);
    }


    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task NotSingleTarget(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var serverItems = factory.Items;

        var receiver = new FakeStreamingHubClientResultTestHubReceiver();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubClientResultTestHub, IStreamingHubClientResultTestHubReceiver>(channel, receiver);

        // Act
        await client.Invoke_Not_SingleTarget();

        // Assert
        Assert.Empty(receiver.Received);
        Assert.Equal("System.NotSupportedException", ((string)serverItems.GetValueOrDefault(nameof(IStreamingHubClientResultTestHub.Invoke_Not_SingleTarget))!));
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

    public async Task<string> Parameter_One(string arg1)
    {
        Received.Add((nameof(Parameter_One), (arg1)));
        await Task.Delay(10);
        return $"{nameof(Parameter_One)}:{arg1}";
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

    public Task<string> Throw_With_StatusCode()
    {
        Received.Add((nameof(Throw_With_StatusCode), (ArgumentEmpty)));
        throw new RpcException(new Status(StatusCode.Unauthenticated, "Something went wrong."));
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

    public async Task<string> Parameter_One_With_Cancellation(string arg1, CancellationToken cancellationToken)
    {
        Received.Add((nameof(Parameter_One_With_Cancellation), (arg1, cancellationToken)));
        await Task.Delay(1000);
        return $"{nameof(Parameter_One_With_Cancellation)}:{arg1},{cancellationToken.CanBeCanceled}";
    }

    public async Task<string> Parameter_One_With_Cancellation_Optional(string arg1, CancellationToken cancellationToken = default)
    {
        Received.Add((nameof(Parameter_One_With_Cancellation_Optional), (arg1, cancellationToken)));
        await Task.Delay(1000);
        return $"{nameof(Parameter_One_With_Cancellation_Optional)}:{arg1},{cancellationToken.CanBeCanceled}";
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
    Task Invoke_Parameter_One();
    Task Invoke_Parameter_Many();
    Task Invoke_Throw();
    Task Invoke_Throw_With_StatusCode();
    Task Invoke_Parameter_Zero_With_Cancellation();
    Task Invoke_Parameter_Zero_With_Cancellation_Optional();
    Task Invoke_Parameter_One_With_Cancellation();
    Task Invoke_Parameter_One_With_Cancellation_Optional();
    Task Invoke_Parameter_Many_With_Cancellation();
    Task Invoke_Parameter_Many_With_Cancellation_Optional();
    Task Invoke_After_Disconnected();
    Task Invoke_Not_SingleTarget();
}

public interface IStreamingHubClientResultTestHubReceiver
{
    Task Parameter_Zero_NoResultValue();
    Task<string> Parameter_Zero();
    Task<string> Parameter_One(string arg1);
    Task<string> Parameter_Many(string arg1, int arg2, bool arg3);
    Task<string> Throw();
    Task<string> Throw_With_StatusCode();
    Task<string> Parameter_Zero_With_Cancellation(CancellationToken cancellationToken);
    Task<string> Parameter_Zero_With_Cancellation_Optional(CancellationToken cancellationToken = default);
    Task<string> Parameter_One_With_Cancellation(string arg1, CancellationToken cancellationToken);
    Task<string> Parameter_One_With_Cancellation_Optional(string arg1, CancellationToken cancellationToken = default);
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

    public async Task Invoke_Parameter_One()
    {
        var result = await Client.Parameter_One("Hello");
        Items.TryAdd(nameof(Invoke_Parameter_One), result);
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

    public async Task Invoke_Throw_With_StatusCode()
    {
        try
        {
            var result = await Client.Throw_With_StatusCode();
        }
        catch (RpcException e)
        {
            Items.TryAdd(nameof(Invoke_Throw_With_StatusCode), (e.GetType().FullName, e.Message));
            Items.TryAdd(nameof(Invoke_Throw_With_StatusCode) + "/StatusCode", e.StatusCode);
        }
    }

    public async Task Invoke_After_Disconnected()
    {
        try
        {
            await ((SemaphoreSlim)Items[nameof(Invoke_After_Disconnected) + "/Signal/FromClient"]).WaitAsync();
            //Context.CallContext.GetHttpContext().Abort();
            var result = await Client.Parameter_Zero();
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_After_Disconnected), (e.GetType().FullName, e.Message));
            Items.TryAdd(nameof(Invoke_After_Disconnected) + "/Exception", (e.ToString()));
        }
        finally
        {
            ((SemaphoreSlim)Items[nameof(Invoke_After_Disconnected) + "/Signal/ToClient"]).Release();
        }
    }

    public async Task Invoke_Parameter_Zero_With_Cancellation()
    {
        var cts = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_Zero_With_Cancellation(cts.Token);
            Items.TryAdd(nameof(Invoke_Parameter_Zero_With_Cancellation), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_Zero_With_Cancellation), (e.GetType().FullName!));
        }
    }

    public async Task Invoke_Parameter_Zero_With_Cancellation_Optional()
    {
        var cts = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_Zero_With_Cancellation_Optional(cts.Token);
            Items.TryAdd(nameof(Invoke_Parameter_Zero_With_Cancellation), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_Zero_With_Cancellation_Optional), (e.GetType().FullName!));
        }
    }

    public async Task Invoke_Parameter_One_With_Cancellation()
    {
        var cts = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_One_With_Cancellation("Hello", cts.Token);
            Items.TryAdd(nameof(Invoke_Parameter_One_With_Cancellation), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_One_With_Cancellation), (e.GetType().FullName!));
        }
    }

    public async Task Invoke_Parameter_One_With_Cancellation_Optional()
    {
        var cts = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_One_With_Cancellation_Optional("Hello", cts.Token);
            Items.TryAdd(nameof(Invoke_Parameter_One_With_Cancellation_Optional), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_One_With_Cancellation_Optional), (e.GetType().FullName!));
        }
    }

    public async Task Invoke_Parameter_Many_With_Cancellation()
    {
        var cts = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_Many_With_Cancellation("Hello", 12345, true, cts.Token);
            Items.TryAdd(nameof(Invoke_Parameter_Many_With_Cancellation), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_Many_With_Cancellation), (e.GetType().FullName!));
        }
    }

    public async Task Invoke_Parameter_Many_With_Cancellation_Optional()
    {
        var cts = new CancellationTokenSource(250);
        try
        {
            var result = await Client.Parameter_Many_With_Cancellation_Optional("Hello", 12345, true, cts.Token);
            Items.TryAdd(nameof(Invoke_Parameter_Many_With_Cancellation_Optional), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Parameter_Many_With_Cancellation_Optional), (e.GetType().FullName!));
        }
    }

    public async Task Invoke_Not_SingleTarget()
    {
        try
        {
            var group = await Group.AddAsync(nameof(Invoke_Not_SingleTarget) + Guid.NewGuid());

            var result = await group.All.Parameter_Zero();
            Items.TryAdd(nameof(Invoke_Not_SingleTarget), (result));
        }
        catch (Exception e)
        {
            Items.TryAdd(nameof(Invoke_Not_SingleTarget), (e.GetType().FullName!));
        }
    }
}
