using System.Collections.Concurrent;
using System.Diagnostics;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Integration.Tests.Generated;
using MagicOnion.Serialization;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Integration.Tests;

public class StreamingHubTest : IClassFixture<MagicOnionApplicationFactory<StreamingHubTestHub>>
{
    readonly MagicOnionApplicationFactory<StreamingHubTestHub> factory;

    public StreamingHubTest(MagicOnionApplicationFactory<StreamingHubTestHub> factory)
    {
        this.factory = factory;
    }

    public static IEnumerable<object[]> EnumerateStreamingHubClientFactory()
    {
        yield return new [] { new TestStreamingHubClientFactory("Dynamic", DynamicStreamingHubClientFactoryProvider.Instance) };
        yield return new [] { new TestStreamingHubClientFactory("Static", MagicOnionGeneratedClientFactoryProvider.Instance)};
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task NoReturn_Parameter_Zero(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act & Assert
        await client.NoReturn_Parameter_Zero();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task NoReturn_Parameter_One(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act & Assert
        await client.NoReturn_Parameter_One(12345);
    }

    
    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task NoReturn_Parameter_Many(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act & Assert
        await client.NoReturn_Parameter_Many(12345, "Hello✨", true);
    }
    
    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Zero(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var result = await client.Parameter_Zero();

        // Assert
        result.Should().Be(67890);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_One(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var result = await client.Parameter_One(12345);

        // Assert
        result.Should().Be(67890);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Parameter_Many(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var result = await client.Parameter_Many(12345, "Hello✨", true);

        // Assert
        result.Should().Be(67890);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task ValueTask_NoReturn_Parameter_Zero(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act & Assert
        await client.ValueTask_NoReturn_Parameter_Zero();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task ValueTask_NoReturn_Parameter_One(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act & Assert
        await client.ValueTask_NoReturn_Parameter_One(12345);
    }

    
    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task ValueTask_NoReturn_Parameter_Many(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act & Assert
        await client.ValueTask_NoReturn_Parameter_Many(12345, "Hello✨", true);
    }
    
    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task ValueTask_Parameter_Zero(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var result = await client.ValueTask_Parameter_Zero();

        // Assert
        result.Should().Be(67890);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task ValueTask_Parameter_One(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var result = await client.ValueTask_Parameter_One(12345);

        // Assert
        result.Should().Be(67890);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task ValueTask_Parameter_Many(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var result = await client.ValueTask_Parameter_Many(12345, "Hello✨", true);

        // Assert
        result.Should().Be(67890);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Receiver_Parameter_Zero(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        await client.CallReceiver_Parameter_Zero();
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_Parameter_Zero();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Receiver_Parameter_One(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        await client.CallReceiver_Parameter_One(12345);
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_Parameter_One(12345);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Receiver_Parameter_Many(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        await client.CallReceiver_Parameter_Many(12345, "Hello✨", true);
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_Parameter_Many(12345, "Hello✨", true);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Forget_NoReturnValue(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);
        client = client.FireAndForget(); // Use FireAndForget client

        // Act
        await client.Never();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Forget_WithReturnValue(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);
        client = client.FireAndForget(); // Use FireAndForget client

        // Act
        var result = await client.Never_With_Return();

        // Assert
        result.Should().Be(default(int));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task ValueTask_Forget_NoReturnValue(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);
        client = client.FireAndForget(); // Use FireAndForget client

        // Act
        await client.ValueTask_Never();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task ValueTask_Forget_WithReturnValue(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);
        client = client.FireAndForget(); // Use FireAndForget client

        // Act
        var result = await client.ValueTask_Never_With_Return();

        // Assert
        result.Should().Be(default(int));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task RefType(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);
        var request = new MyStreamingRequest(123, 456);

        // Act
        var result = await client.RefType(request);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(123 + 456);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task RefType_Null(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var result = await client.RefType_Null(null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Receiver_RefType(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);
        var request = new MyStreamingRequest(123, 456);

        // Act
        await client.CallReceiver_RefType(request);
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_RefType(Arg.Is<MyStreamingResponse>(y => y.Value == 123 + 456));
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Receiver_RefType_Null(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        await client.CallReceiver_RefType_Null();
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_RefType_Null(default);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task ContinuationBlocking(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        // NOTE: Runs on another thread.
        _ = Task.Run(async () =>
        {
            await client.CallReceiver_Delay(500); // The receiver will be called after 500ms.
            Thread.Sleep(60 * 1000); // Block the continuation.
        });
        await Task.Delay(1000); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_Delay();
    }

    [Fact]
    public async Task ThrowReturnStatusException()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var ex = (RpcException?)await Record.ExceptionAsync(async () => await client.ThrowReturnStatusException());

        // Assert
        ex.Should().NotBeNull();
        ex!.StatusCode.Should().Be(StatusCode.Unknown);
        ex.Status.Detail.Should().Be("Detail-String");
    }

    [Fact]
    public async Task Throw()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var ex = (RpcException?)await Record.ExceptionAsync(async () => await client.Throw());

        // Assert
        ex.Should().NotBeNull();
        ex!.StatusCode.Should().Be(StatusCode.Internal);
        ex.Status.Detail.Should().StartWith("An error occurred while processing handler");
    }

    [Fact]
    public async Task Throw_WithServerStackTrace()
    {
        // Arrange
        var factory = this.factory.WithWebHostBuilder(builder => builder.ConfigureServices(services => services.Configure<MagicOnionOptions>(options => options.IsReturnExceptionStackTraceInErrorDetail = true)));
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var ex = (RpcException?)await Record.ExceptionAsync(async () => await client.Throw());

        // Assert
        ex.Should().NotBeNull();
        ex!.StatusCode.Should().Be(StatusCode.Internal);
        ex.Message.Should().Contain("Something went wrong.");
        ex.Status.Detail.Should().StartWith("An error occurred while processing handler");
    }
}

public class StreamingHubTestHub : StreamingHubBase<IStreamingHubTestHub, IStreamingHubTestHubReceiver>, IStreamingHubTestHub
{
    IGroup group = default!;

    protected override async ValueTask OnConnecting()
    {
        group = await Group.AddAsync(ConnectionId.ToString());
    }

    public Task NoReturn_Parameter_Zero()
    {
        return Task.CompletedTask;
    }

    public Task NoReturn_Parameter_One(int arg0)
    {
        Debug.Assert(arg0 == 12345);
        return Task.CompletedTask;
    }

    public Task NoReturn_Parameter_Many(int arg0, string arg1, bool arg2)
    {
        Debug.Assert(arg0 == 12345);
        Debug.Assert(arg1 == "Hello✨");
        Debug.Assert(arg2 == true);
        return Task.CompletedTask;
    }

    public Task<int> Parameter_Zero()
    {
        return Task.FromResult(67890);
    }

    public Task<int> Parameter_One(int arg0)
    {
        Debug.Assert(arg0 == 12345);
        return Task.FromResult(67890);
    }

    public Task<int> Parameter_Many(int arg0, string arg1, bool arg2)
    {
        Debug.Assert(arg0 == 12345);
        Debug.Assert(arg1 == "Hello✨");
        Debug.Assert(arg2 == true);
        return Task.FromResult(67890);
    }

    public Task CallReceiver_Parameter_Zero()
    {
        Broadcast(group).Receiver_Parameter_Zero();
        return Task.CompletedTask;
    }

    public Task CallReceiver_Parameter_One(int arg0)
    {
        Broadcast(group).Receiver_Parameter_One(12345);
        return Task.CompletedTask;
    }

    public Task CallReceiver_Parameter_Many(int arg0, string arg1, bool arg2)
    {
        Broadcast(group).Receiver_Parameter_Many(12345, "Hello✨", true);
        return Task.CompletedTask;
    }

    public Task Never()
    {
        return new TaskCompletionSource().Task.WaitAsync(TimeSpan.FromMilliseconds(100));
    }
    
    public Task<int> Never_With_Return()
    {
        return new TaskCompletionSource<int>().Task.WaitAsync(TimeSpan.FromMilliseconds(100));
    }
    
    public ValueTask ValueTask_NoReturn_Parameter_Zero()
    {
        return default;
    }

    public ValueTask ValueTask_NoReturn_Parameter_One(int arg0)
    {
        Debug.Assert(arg0 == 12345);
        return default;
    }

    public ValueTask ValueTask_NoReturn_Parameter_Many(int arg0, string arg1, bool arg2)
    {
        Debug.Assert(arg0 == 12345);
        Debug.Assert(arg1 == "Hello✨");
        Debug.Assert(arg2 == true);
        return default;
    }

    public ValueTask<int> ValueTask_Parameter_Zero()
    {
        return ValueTask.FromResult(67890);
    }

    public ValueTask<int> ValueTask_Parameter_One(int arg0)
    {
        Debug.Assert(arg0 == 12345);
        return ValueTask.FromResult(67890);
    }

    public ValueTask<int> ValueTask_Parameter_Many(int arg0, string arg1, bool arg2)
    {
        Debug.Assert(arg0 == 12345);
        Debug.Assert(arg1 == "Hello✨");
        Debug.Assert(arg2 == true);
        return ValueTask.FromResult(67890);
    }

    public ValueTask ValueTask_Never()
    {
        return new ValueTask(new TaskCompletionSource().Task.WaitAsync(TimeSpan.FromMilliseconds(100)));
    }
    
    public ValueTask<int> ValueTask_Never_With_Return()
    {
        return new ValueTask<int>(new TaskCompletionSource<int>().Task.WaitAsync(TimeSpan.FromMilliseconds(100)));
    }

    public Task<MyStreamingResponse> RefType(MyStreamingRequest request)
    {
        return Task.FromResult(new MyStreamingResponse(request.Argument0 + request.Argument1));
    }

    public Task<MyStreamingResponse?> RefType_Null(MyStreamingRequest? request)
    {
        Debug.Assert(request is null);
        return Task.FromResult(default(MyStreamingResponse));
    }

    public Task CallReceiver_RefType(MyStreamingRequest request)
    {
        Broadcast(group).Receiver_RefType(new MyStreamingResponse(request.Argument0 + request.Argument1));
        return Task.CompletedTask;
    }

    public Task CallReceiver_RefType_Null()
    {
        Broadcast(group).Receiver_RefType_Null(default);
        return Task.CompletedTask;
    }

    public Task CallReceiver_Delay(int milliseconds)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(milliseconds);
            Broadcast(group).Receiver_Delay();
        });

        return Task.CompletedTask;
    }

    public Task ThrowReturnStatusException()
    {
        throw new ReturnStatusException(StatusCode.Unknown, "Detail-String");
    }

    public Task Throw()
    {
        throw new InvalidOperationException("Something went wrong.");
    }
}

public interface IStreamingHubTestHubReceiver
{
    void Receiver_Parameter_Zero();
    void Receiver_Parameter_One(int arg0);
    void Receiver_Parameter_Many(int arg0, string arg1, bool arg2);
    void Receiver_RefType(MyStreamingResponse request);
    void Receiver_RefType_Null(MyStreamingResponse? request);
    void Receiver_Delay();
}

public interface IStreamingHubTestHub : IStreamingHub<IStreamingHubTestHub, IStreamingHubTestHubReceiver>
{
    Task NoReturn_Parameter_Zero();
    Task NoReturn_Parameter_One(int arg0);
    Task NoReturn_Parameter_Many(int arg0, string arg1, bool arg2);

    Task<int> Parameter_Zero();
    Task<int> Parameter_One(int arg0);
    Task<int> Parameter_Many(int arg0, string arg1, bool arg2);

    Task CallReceiver_Parameter_Zero();
    Task CallReceiver_Parameter_One(int arg0);
    Task CallReceiver_Parameter_Many(int arg0, string arg1, bool arg2);

    Task Never();
    Task<int> Never_With_Return();

    ValueTask ValueTask_NoReturn_Parameter_Zero();
    ValueTask ValueTask_NoReturn_Parameter_One(int arg0);
    ValueTask ValueTask_NoReturn_Parameter_Many(int arg0, string arg1, bool arg2);

    ValueTask<int> ValueTask_Parameter_Zero();
    ValueTask<int> ValueTask_Parameter_One(int arg0);
    ValueTask<int> ValueTask_Parameter_Many(int arg0, string arg1, bool arg2);

    ValueTask ValueTask_Never();
    ValueTask<int> ValueTask_Never_With_Return();

    Task<MyStreamingResponse> RefType(MyStreamingRequest request);
    Task<MyStreamingResponse?> RefType_Null(MyStreamingRequest? request);
    Task CallReceiver_RefType(MyStreamingRequest request);
    Task CallReceiver_RefType_Null();

    Task CallReceiver_Delay(int milliseconds);

    Task ThrowReturnStatusException();
    Task Throw();
}
