using System.Diagnostics;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Integration.Tests;

public partial class StreamingHubTest : IClassFixture<MagicOnionApplicationFactory<StreamingHubTestHub>>
{
    readonly MagicOnionApplicationFactory<StreamingHubTestHub> factory;

    public StreamingHubTest(MagicOnionApplicationFactory<StreamingHubTestHub> factory)
    {
        this.factory = factory;
    }

    public static IEnumerable<object[]> EnumerateStreamingHubClientFactory()
    {
        yield return new [] { new TestStreamingHubClientFactory("Dynamic", DynamicStreamingHubClientFactoryProvider.Instance) };
        yield return new [] { new TestStreamingHubClientFactory("Static", MagicOnionGeneratedClientInitializer.StreamingHubClientFactoryProvider)};
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
        Assert.Equal(67890, result);
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
        Assert.Equal(67890, result);
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
        Assert.Equal(67890, result);
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
        Assert.Equal(67890, result);
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
        Assert.Equal(67890, result);
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
        Assert.Equal(67890, result);
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
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

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
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

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
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

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
        Assert.Equal(default(int), result);
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
        Assert.Equal(default(int), result);
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
        Assert.NotNull(result);
        Assert.Equal(123 + 456, result.Value);
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
        Assert.Null(result);
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
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

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
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

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
        }, TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_Delay();
    }

    [Fact]
    public async Task ThrowReturnStatusException()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var ex = (RpcException?)await Record.ExceptionAsync(() => client.ThrowReturnStatusException());

        // Assert
        Assert.NotNull(ex);
        Assert.Equal(StatusCode.Unknown, ex!.StatusCode);
        Assert.Equal("Detail-String", ex.Status.Detail);
    }

    [Fact]
    public async Task Throw()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var ex = (RpcException?)await Record.ExceptionAsync(() => client.Throw());

        // Assert
        Assert.NotNull(ex);
        Assert.Equal(StatusCode.Internal, ex!.StatusCode);
        Assert.StartsWith("An error occurred while processing handler", ex.Status.Detail);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Concurrency(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cts = new CancellationTokenSource();
        var tasks = Enumerable.Range(0, 10).Select(async x =>
        {
            var httpClient = factory.CreateDefaultClient();
            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
            var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
            var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

            await tcs.Task;

            var semaphore = new SemaphoreSlim(0);
            var results = new List<(int Index, (int Arg0, string Arg1, bool Arg2) Request, (int Arg0, string Arg1, bool Arg2) Response)>();
            var receiverResults = new List<(int Arg0, string Arg1, bool Arg2)>();
            receiver.When(x => x.Receiver_Concurrent(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>()))
                .Do(x =>
                {
                    receiverResults.Add((x.ArgAt<int>(0), x.ArgAt<string>(1), x.ArgAt<bool>(2)));
                    semaphore.Release();
                });

            var count = 0;
            while (!cts.IsCancellationRequested)
            {
                var response = await client.Concurrent((x * 100) + count, $"Task{x}-{count}", x % 2 == 0);
                results.Add((Index: count, Request: ((x * 100) + count, $"Task{x}-{count}", x % 2 == 0), Response: response));
                await semaphore.WaitAsync(TestContext.Current.CancellationToken);

                count++;
            }

            await Task.Delay(1000, TestContext.Current.CancellationToken);

            return (Sequence: x, Results: results, ReceiverResults: receiverResults);
        });

        // Act
        tcs.TrySetResult();
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        var allResults = await Task.WhenAll(tasks);

        // Assert
        Assert.All(allResults, x =>
        {
            Assert.Equal(x.Results.Count, x.ReceiverResults.Count);
            Assert.All(x.Results, y => Assert.Equal((y.Request.Arg0 * 100, y.Request.Arg1 + "-Result", !y.Request.Arg2), y.Response));
            for (var i = 0; i < x.ReceiverResults.Count; i++)
            {
                var request = x.Results[i];
                Assert.Equal((request.Request.Arg0 * 10, request.Request.Arg1 + "-Receiver", !request.Request.Arg2), x.ReceiverResults[i]);
            }
        });
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Void_Parameter_Zero(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        client.Void_Parameter_Zero();
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_Test_Void_Parameter_Zero();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Void_Parameter_One(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        client.Void_Parameter_One(12345);
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_Test_Void_Parameter_One(12345);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Void_Parameter_Many(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        client.Void_Parameter_Many(12345, "Hello✨", true);
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_Test_Void_Parameter_Many(12345, "Hello✨", true);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task Process_Requests_Sequentially(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        var task1 = client.Delay(1, TimeSpan.FromSeconds(1.5));
        var task2 = client.Delay(2, TimeSpan.FromSeconds(1));
        var task3 = client.Delay(3, TimeSpan.FromSeconds(0.5));

        // Assert
        Assert.Equal(1, await task1);
        Assert.False(task2.IsCompleted);
        Assert.False(task3.IsCompleted);

        Assert.Equal(2, await task2);
        Assert.False(task3.IsCompleted);

        Assert.Equal(3, await task3);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task CustomMethodId(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act & Assert
        await client.CustomMethodId();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task CustomMethodId_Receiver(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        await client.CallReceiver_CustomMethodId();
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_CustomMethodId();
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task WaitForDisconnectAsync_CompletedNormally(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        await client.DisposeAsync();
        var reason = await client.WaitForDisconnectAsync();

        // Assert
        Assert.Equal(DisconnectionType.CompletedNormally, reason.Type);
        Assert.Null(reason.Exception);
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task WaitForDisconnectAsync_Faulted(TestStreamingHubClientFactory clientFactory)
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await clientFactory.CreateAndConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(channel, receiver);

        // Act
        try
        {
            await client.DisconnectFromServerAsync();
        }
        catch { }
        var reason = await client.WaitForDisconnectAsync();

        // Assert
        Assert.Equal(DisconnectionType.Faulted, reason.Type);
        Assert.NotNull(reason.Exception);
    }
}

public class StreamingHubTestHub : StreamingHubBase<IStreamingHubTestHub, IStreamingHubTestHubReceiver>, IStreamingHubTestHub
{
    IGroup<IStreamingHubTestHubReceiver> group = default!;

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
        group.All.Receiver_Parameter_Zero();
        return Task.CompletedTask;
    }

    public Task CallReceiver_Parameter_One(int arg0)
    {
        group.All.Receiver_Parameter_One(12345);
        return Task.CompletedTask;
    }

    public Task CallReceiver_Parameter_Many(int arg0, string arg1, bool arg2)
    {
        group.All.Receiver_Parameter_Many(12345, "Hello✨", true);
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

    public void Void_Parameter_Zero()
    {
        group.All.Receiver_Test_Void_Parameter_Zero();
    }

    public void Void_Parameter_One(int arg0)
    {
        Debug.Assert(arg0 == 12345);
        group.All.Receiver_Test_Void_Parameter_One(arg0);
    }

    public void Void_Parameter_Many(int arg0, string arg1, bool arg2)
    {
        Debug.Assert(arg0 == 12345);
        Debug.Assert(arg1 == "Hello✨");
        Debug.Assert(arg2 == true);
        group.All.Receiver_Test_Void_Parameter_Many(arg0, arg1, arg2);
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
        group.All.Receiver_RefType(new MyStreamingResponse(request.Argument0 + request.Argument1));
        return Task.CompletedTask;
    }

    public Task CallReceiver_RefType_Null()
    {
        group.All.Receiver_RefType_Null(default);
        return Task.CompletedTask;
    }

    public Task CallReceiver_Delay(int milliseconds)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(milliseconds);
            group.All.Receiver_Delay();
        });

        return Task.CompletedTask;
    }

    public Task CallReceiver_CustomMethodId()
    {
        group.All.Receiver_CustomMethodId();
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

    public async Task<(int, string, bool)> Concurrent(int arg0, string arg1, bool arg2)
    {
        group.All.Receiver_Concurrent(arg0 * 10, arg1 + "-Receiver", !arg2);
        await Task.Yield();
        return (arg0 * 100, arg1 + "-Result", !arg2);
    }

    public async Task<int> Delay(int id, TimeSpan delay)
    {
        await Task.Delay(delay);
        return id;
    }

    public Task CustomMethodId()
    {
        return Task.CompletedTask;
    }

    public Task DisconnectFromServerAsync()
    {
        this.Context.CallContext.GetHttpContext().Abort();
        return Task.CompletedTask;
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
    void Receiver_Concurrent(int arg0, string arg1, bool arg2);

    void Receiver_Test_Void_Parameter_Zero();
    void Receiver_Test_Void_Parameter_One(int arg0);
    void Receiver_Test_Void_Parameter_Many(int arg0, string arg1, bool arg2);

    [MethodId(54321)]
    void Receiver_CustomMethodId();
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

    void Void_Parameter_Zero();
    void Void_Parameter_One(int arg0);
    void Void_Parameter_Many(int arg0, string arg1, bool arg2);

    Task<MyStreamingResponse> RefType(MyStreamingRequest request);
    Task<MyStreamingResponse?> RefType_Null(MyStreamingRequest? request);
    Task CallReceiver_RefType(MyStreamingRequest request);
    Task CallReceiver_RefType_Null();

    Task CallReceiver_Delay(int milliseconds);

    Task CallReceiver_CustomMethodId();

    Task ThrowReturnStatusException();
    Task Throw();

    Task<(int Arg0, string Arg1, bool Arg2)> Concurrent(int arg0, string arg1, bool arg2);

    Task<int> Delay(int id, TimeSpan delay);

    [MethodId(12345)]
    Task CustomMethodId();

    Task DisconnectFromServerAsync();
}
