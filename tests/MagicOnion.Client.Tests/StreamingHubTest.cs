using MagicOnion.Client.DynamicClient;
using Microsoft.Extensions.Time.Testing;
using System.Buffers;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MagicOnion.Client.Tests;

public class StreamingHubTest
{
    [Fact]
    public async Task Task_NoReturnValue_Parameter_Zero()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.NoReturnValue_Parameter_Zero().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<Nil>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, Nil.Default);

        // Wait for hub method completion.
        await t;

        Assert.Equal(Nil.Default, requestBody);
    }

    [Fact]
    public async Task Task_Parameter_Zero()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.Parameter_Zero().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<Nil>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, 123);

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(Nil.Default, requestBody);
        Assert.Equal(123, result);
    }

    [Fact]
    public async Task Task_Parameter_One()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.Parameter_One(12345).WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<int>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, 456);

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(12345, requestBody);
        Assert.Equal(456, result);
    }

    [Fact]
    public async Task Task_Parameter_Two()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.Parameter_Two(12345, "Hello").WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<DynamicArgumentTuple<int, string>>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, 456);

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(12345, requestBody.Item1);
        Assert.Equal("Hello", requestBody.Item2);
        Assert.Equal(456, result);
    }

    [Fact]
    public async Task Task_Parameter_Many()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.Parameter_Many(12345, "Hello", 0xfffffff1L, true, 128, 12.345, 'X').WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<DynamicArgumentTuple<int, string, long, bool, byte, double, char>>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, 456);

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(12345, requestBody.Item1);
        Assert.Equal("Hello", requestBody.Item2);
        Assert.Equal(0xfffffff1L, requestBody.Item3);
        Assert.True(requestBody.Item4);
        Assert.Equal(128, requestBody.Item5);
        Assert.Equal(12.345, requestBody.Item6);
        Assert.Equal('X', requestBody.Item7);
        Assert.Equal(456, result);
    }

    [Fact]
    public async Task Task_Forget_NoReturnValue_Parameter_Zero()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Use Fire-and-forget client
        client = client.FireAndForget();

        // Invoke Hub Method
        var t = client.NoReturnValue_Parameter_Zero().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (methodId, requestBody) = await helper.ReadFireAndForgetRequestAsync<Nil>();

        // Wait for hub method completion.
        await t;

        Assert.Equal(Nil.Default, requestBody);
    }

    [Fact]
    public async Task Task_Forget_Parameter_Zero()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Use Fire-and-forget client
        client = client.FireAndForget();

        // Invoke Hub Method
        var t = client.Parameter_Zero().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (methodId, requestBody) = await helper.ReadFireAndForgetRequestAsync<Nil>();

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(Nil.Default, requestBody);
        Assert.Equal(default, result);
    }

    [Fact]
    public async Task Task_Forget_Parameter_One()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Use Fire-and-forget client
        client = client.FireAndForget();

        // Invoke Hub Method
        var t = client.Parameter_One(123).WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (methodId, requestBody) = await helper.ReadFireAndForgetRequestAsync<int>();

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(123, requestBody);
        Assert.Equal(default, result);
    }

    [Fact]
    public async Task ValueTask_NoReturnValue_Parameter_Zero()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.ValueTask_NoReturnValue_Parameter_Zero().AsTask().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<Nil>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, Nil.Default);

        // Wait for hub method completion.
        await t;

        Assert.Equal(Nil.Default, requestBody);
    }

    [Fact]
    public async Task ValueTask_Parameter_Zero()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.ValueTask_Parameter_Zero().AsTask().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<Nil>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, 123);

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(Nil.Default, requestBody);
        Assert.Equal(123, result);
    }

    [Fact]
    public async Task ValueTask_Parameter_One()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.ValueTask_Parameter_One(12345).AsTask().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<int>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, 456);

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(12345, requestBody);
        Assert.Equal(456, result);
    }

    [Fact]
    public async Task ValueTask_Parameter_Two()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.ValueTask_Parameter_Two(12345, "Hello").AsTask().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<DynamicArgumentTuple<int, string>>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, 456);

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(12345, requestBody.Item1);
        Assert.Equal("Hello", requestBody.Item2);
        Assert.Equal(456, result);
    }

    [Fact]
    public async Task ValueTask_Parameter_Many()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        var t = client.ValueTask_Parameter_Many(12345, "Hello", 0xfffffff1L, true, 128, 12.345, 'X').AsTask().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<DynamicArgumentTuple<int, string, long, bool, byte, double, char>>();
        // Write a response to the stream
        helper.WriteResponse(messageId, methodId, 456);

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(12345, requestBody.Item1);
        Assert.Equal("Hello", requestBody.Item2);
        Assert.Equal(0xfffffff1L, requestBody.Item3);
        Assert.True(requestBody.Item4);
        Assert.Equal(128, requestBody.Item5);
        Assert.Equal(12.345, requestBody.Item6);
        Assert.Equal('X', requestBody.Item7);
        Assert.Equal(456, result);
    }

    [Fact]
    public async Task ValueTask_Forget_NoReturnValue_Parameter_Zero()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Use Fire-and-forget client
        client = client.FireAndForget();

        // Invoke Hub Method
        var t = client.ValueTask_NoReturnValue_Parameter_Zero().AsTask().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (methodId, requestBody) = await helper.ReadFireAndForgetRequestAsync<Nil>();

        // Wait for hub method completion.
        await t;

        Assert.Equal(Nil.Default, requestBody);
    }

    [Fact]
    public async Task ValueTask_Forget_Parameter_Zero()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Use Fire-and-forget client
        client = client.FireAndForget();

        // Invoke Hub Method
        var t = client.ValueTask_Parameter_Zero().AsTask().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (methodId, requestBody) = await helper.ReadFireAndForgetRequestAsync<Nil>();

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(Nil.Default, requestBody);
        Assert.Equal(default, result);
    }

    [Fact]
    public async Task ValueTask_Forget_Parameter_One()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Use Fire-and-forget client
        client = client.FireAndForget();

        // Invoke Hub Method
        var t = client.ValueTask_Parameter_One(123).AsTask().WaitAsync(timeout.Token);

        // Read a hub method request payload
        var (methodId, requestBody) = await helper.ReadFireAndForgetRequestAsync<int>();

        // Wait for hub method completion.
        var result = await t;

        Assert.Equal(123, requestBody);
        Assert.Equal(default, result);
    }

    [Fact]
    public async Task Void_Parameter_Zero()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Use Fire-and-forget client
        client = client.FireAndForget();

        // Invoke Hub Method
        client.Void_Parameter_Zero();

        // Read a hub method request payload
        var (methodId, requestBody) = await helper.ReadFireAndForgetRequestAsync<Nil>();

        Assert.Equal(Nil.Default, requestBody);
    }

    [Fact]
    public async Task Void_Parameter_One()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Use Fire-and-forget client
        client = client.FireAndForget();

        // Invoke Hub Method
        client.Void_Parameter_One(123);

        // Read a hub method request payload
        var (methodId, requestBody) = await helper.ReadFireAndForgetRequestAsync<int>();

        Assert.Equal(123, requestBody);
    }

    [Fact]
    public async Task Void_Parameter_Many()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Invoke Hub Method
        client.Void_Parameter_Many(12345, "Hello", 0xfffffff1L, true, 128, 12.345, 'X');

        // Read a hub method request payload
        var (methodId, requestBody) = await helper.ReadFireAndForgetRequestAsync<DynamicArgumentTuple<int, string, long, bool, byte, double, char>>();

        Assert.Equal(12345, requestBody.Item1);
        Assert.Equal("Hello", requestBody.Item2);
        Assert.Equal(0xfffffff1L, requestBody.Item3);
        Assert.True(requestBody.Item4);
        Assert.Equal(128, requestBody.Item5);
        Assert.Equal(12.345, requestBody.Item6);
        Assert.Equal('X', requestBody.Item7);
    }

    [Fact]
    public async Task ClientHeartbeat_Interval()
    {
        // Arrange
        var origin = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(origin);
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var options = StreamingHubClientOptions.CreateWithDefault().WithClientHeartbeatInterval(TimeSpan.FromMilliseconds(100)).WithTimeProvider(timeProvider);
        var client = await helper.ConnectAsync(options, timeout.Token);

        // Act
        var t = client.Parameter_One(1234);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(100, TestContext.Current.CancellationToken); // Wait for processing queue.
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(100, TestContext.Current.CancellationToken); // Wait for processing queue.
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(100, TestContext.Current.CancellationToken); // Wait for processing queue.

        // Assert
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<int>();
        Assert.Equal(1234, requestBody);

        var request1 = await helper.ReadRequestRawAsync();
        var request2 = await helper.ReadRequestRawAsync();
        var request3 = await helper.ReadRequestRawAsync();
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x00 /* Sequence(0) */, .. ToMessagePackBytes(TimeSpan.FromMilliseconds(100)) /* ClientSentAt */, 0xc0 /* Nil */], request1.ToArray());
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x01 /* Sequence(1) */, .. ToMessagePackBytes(TimeSpan.FromMilliseconds(200)) /* CliSentAt */, 0xc0 /* Nil */], request2.ToArray());
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0x02 /* Sequence(2) */, .. ToMessagePackBytes(TimeSpan.FromMilliseconds(300)) /* CliSentAt */, 0xc0 /* Nil */], request3.ToArray());

        static byte[] ToMessagePackBytes(TimeSpan ts)
        {
            var ms = (long)ts.TotalMilliseconds;

            var arrayBufferWriter = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(arrayBufferWriter);
            writer.Write(ms);
            writer.Flush();
            return arrayBufferWriter.WrittenMemory.ToArray();
        }
    }

    [Fact]
    public async Task ClientHeartbeat_FirstTime()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var timeProvider = new FakeTimeProvider();
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var options = StreamingHubClientOptions.CreateWithDefault()
            .WithTimeProvider(timeProvider)
            .WithClientHeartbeatTimeout(TimeSpan.FromSeconds(1))
            .WithClientHeartbeatInterval(TimeSpan.FromSeconds(1));
        var client = await helper.ConnectAsync(options, timeout.Token);

        // Act
        var waitForDisconnectTask = client.WaitForDisconnect();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(100, TestContext.Current.CancellationToken); // Wait for processing queue.
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(100, TestContext.Current.CancellationToken); // Wait for processing queue.
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(100, TestContext.Current.CancellationToken); // Wait for processing queue.
        await waitForDisconnectTask.WaitAsync(timeout.Token);

        // Assert
        Assert.True(waitForDisconnectTask.IsCompletedSuccessfully); // the client should be timed-out by heartbeat timer.
    }

    [Fact]
    public async Task ServerHeartbeat_Respond()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var options = StreamingHubClientOptions.CreateWithDefault().WithClientHeartbeatInterval(Timeout.InfiniteTimeSpan); // Disable Client Heartbeat timer.
        var client = await helper.ConnectAsync(options, timeout.Token);

        // Act
        var t = client.Parameter_One(1234);
        helper.WriteResponseRaw([0x95 /* Array(5) */, 0x7f /* Type:127 */, 0x00 /* Sequence(0) */, .. (byte[])[0xcd, 0x30, 0x39] /* ServerSentAt */, 0xc0 /* Nil */, 0xc0 /* Extra */]); // Simulate heartbeat from the server.
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<int>();
        Assert.Equal(1234, requestBody);

        var request1 = await helper.ReadRequestRawAsync();
        Assert.Equal((byte[])[0x94, 0x7f, 0x00 /* Sequence(0) */, .. (byte[])[0xcd, 0x30, 0x39] /* ServerSentAt */, 0xc0 /* Nil */], request1.ToArray()); // Respond to the heartbeat from the server.
    }

    [Fact]
    public async Task ServerHeartbeat_ServerTime()
    {
        // Arrange
        var received = default(DateTimeOffset);
        var timeProvider = new FakeTimeProvider();
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var options = StreamingHubClientOptions.CreateWithDefault()
            .WithTimeProvider(timeProvider)
            .WithServerHeartbeatReceived(x => received = x.ServerTime)
            .WithClientHeartbeatInterval(Timeout.InfiniteTimeSpan); // Disable Heartbeat timer.
        var client = await helper.ConnectAsync(options, timeout.Token);

        // Act
        helper.WriteResponseRaw((byte[])[0x95 /* Array(5) */, 0x7f /* Type:127 */, 0x00 /* Sequence(0) */, .. (byte[])[0xcf, 0x00, 0x00, 0x01, 0x90, 0x6b, 0x97, 0x5c, 0x00] /* ServerSentAt */, 0xc0 /* Nil */, 0xc0 /* Extra(Nil) */]); // Simulate heartbeat from the server.
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert
        var request1 = await helper.ReadRequestRawAsync();
        Assert.Equal(new DateTimeOffset(2024, 7, 1, 0, 0, 0, 0, TimeSpan.Zero), received);
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7f /* Type:127 */, 0x00 /* Sequence(0) */, .. (byte[])[0xcf, 0x00, 0x00, 0x01, 0x90, 0x6b, 0x97, 0x5c, 0x00] /* ServerSentAt */, 0xc0 /* Nil */], request1.ToArray()); // Respond to the heartbeat from the server.
    }

    [Fact]
    public async Task ServerHeartbeat_Extra()
    {
        // Arrange
        var received = Array.Empty<byte>();
        var timeProvider = new FakeTimeProvider();
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var options = StreamingHubClientOptions.CreateWithDefault()
            .WithTimeProvider(timeProvider)
            .WithServerHeartbeatReceived(x => received = x.Metadata.ToArray())
            .WithClientHeartbeatInterval(Timeout.InfiniteTimeSpan); // Disable Heartbeat timer.
        var client = await helper.ConnectAsync(options, timeout.Token);

        // Act
        helper.WriteResponseRaw((byte[])[0x95 /* Array(5) */, 0x7f /* Type:127 */, 0x00 /* Sequence(0) */, .. (byte[])[ 0xcd, 0x30, 0x39 ] /* ServerSentAt */, 0xc0 /* Nil */, .."Hello World"u8 /* Extra */]); // Simulate heartbeat from the server.
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([.. "Hello World"u8], received); // Respond to the heartbeat from the server.
        var request1 = await helper.ReadRequestRawAsync();
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7f /* Type:127 */, 0x00 /* Sequence(0) */, .. (byte[])[0xcd, 0x30, 0x39] /* ServerSentAt */, 0xc0 /* Nil */], request1.ToArray()); // Respond to the heartbeat from the server.
    }

    [Fact]
    public async Task WaitForDisconnectAsync_CompletedNormally()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Act
        var waitForDisconnectTask = client.WaitForDisconnectAsync();
        await client.DisposeAsync(); // Complete request and disconnect from the server.
        var disconnectionReason = await waitForDisconnectTask.WaitAsync(timeout.Token);

        // Assert
        Assert.Equal(DisconnectionType.CompletedNormally, disconnectionReason.Type);
        Assert.Null(disconnectionReason.Exception);
    }

    [Fact]
    public async Task WaitForDisconnectAsync_TimedOut()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var timeProvider = new FakeTimeProvider();
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var options = StreamingHubClientOptions.CreateWithDefault()
            .WithTimeProvider(timeProvider)
            .WithClientHeartbeatTimeout(TimeSpan.FromSeconds(1))
            .WithClientHeartbeatInterval(TimeSpan.FromSeconds(1));
        var client = await helper.ConnectAsync(options, timeout.Token);

        // Act
        var waitForDisconnectTask = client.WaitForDisconnectAsync();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(100, TestContext.Current.CancellationToken);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(100, TestContext.Current.CancellationToken);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(100, TestContext.Current.CancellationToken);
        var disconnectionReason = await waitForDisconnectTask.WaitAsync(timeout.Token);

        // Assert
        Assert.Equal(DisconnectionType.TimedOut, disconnectionReason.Type);
        Assert.IsType<RpcException>(disconnectionReason.Exception);
    }

    [Fact]
    public async Task WaitForDisconnectAsync_Faulted()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Act
        var waitForDisconnectTask = client.WaitForDisconnectAsync();
        helper.ThrowIOException();
        var disconnectionReason = await waitForDisconnectTask.WaitAsync(timeout.Token);

        // Assert
        Assert.Equal(DisconnectionType.Faulted, disconnectionReason.Type);
        Assert.IsType<RpcException>(disconnectionReason.Exception);
    }

    [Fact]
    public async Task ThrowRpcException_After_Disconnected()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);
        // Set the client's connection status to “Disconnected”.
        helper.ThrowRpcException();
        var disconnectionReason = await client.WaitForDisconnectAsync().WaitAsync(timeout.Token);

        // Act
        var ex = await Record.ExceptionAsync(async () => await client.Parameter_Zero());

        // Assert
        Assert.IsType<RpcException>(ex);
        Assert.Contains("StreamingHubClient has already been disconnected from the server.", ex.Message);
    }

    [Fact]
    public async Task ThrowObjectDisposedException_After_Disposed()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);
        // Set the client's connection status to “Disconnected”.
        helper.ThrowRpcException();
        var disconnectionReason = await client.WaitForDisconnectAsync().WaitAsync(timeout.Token);

        // Act
        await client.DisposeAsync();
        var ex = await Record.ExceptionAsync(async () => await client.Parameter_Zero());

        // Assert
        var ode = Assert.IsType<ObjectDisposedException>(ex);
        Assert.Equal(nameof(StreamingHubClient), ode.ObjectName);
        Assert.Contains("StreamingHubClient has already been disconnected from the server.", ex.Message);
    }

    [Fact]
    public async Task Cancel_While_WritingStream()
    {
        // Arrange
        AggregateException? unobservedException = default;
        EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionEventHandler = (sender, e) =>
        {
            unobservedException = e.Exception;
        };
        try
        {
            TaskScheduler.UnobservedTaskException += unobservedTaskExceptionEventHandler;
            await CoreAsync();
            await Task.Delay(100, TestContext.Current.CancellationToken);
            GC.Collect();
            GC.Collect();
            GC.Collect();
            GC.Collect();
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= unobservedTaskExceptionEventHandler;
        }

        var ex = unobservedException?.InnerExceptions.FirstOrDefault(x => x.TargetSite?.Name == "MoveNext" && (x.TargetSite?.DeclaringType?.FullName?.StartsWith("MagicOnion.Client.Tests.ChannelClientStreamWriter") ?? false));

        Assert.Null(ex);

        static async Task CoreAsync()
        {
            var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
            var client = await helper.ConnectAsync(timeout.Token);

            _ = Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        _ = client.Parameter_One(0);
                    }
                }
                catch
                {
                    // Ignore exception
                }
            }, TestContext.Current.CancellationToken);
            await Task.Delay(100, TestContext.Current.CancellationToken);
            await client.DisposeAsync();
        }
    }

    [Fact]
    public async Task ConnectAsync_Failure()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(
            factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance,
            onResponseHeaderAsync: metadata => throw new RpcException(new Status(StatusCode.Internal, "Something went wrong.")));

        // Act
        var ex = await Record.ExceptionAsync(async () => await helper.ConnectAsync(timeout.Token));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<RpcException>(ex);
    }

    [Fact]
    public async Task ConnectAsync_CancellationToken_Timeout()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var connectTimeout = new CancellationTokenSource();
        var disposed = true;
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(
            factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance,
            onResponseHeaderAsync: async metadata =>
            {
                // Simulate a long time to response
                await Task.Delay(1500, TestContext.Current.CancellationToken);
            },
            onDuplexStreamingCallDisposeAction: () =>
            {
                disposed = true;
            });

        // Act
        connectTimeout.CancelAfter(TimeSpan.FromSeconds(1));
        var begin = Stopwatch.GetTimestamp();
        var ex = await Record.ExceptionAsync(async () => await helper.ConnectAsync(connectTimeout.Token));
        var elapsed = Stopwatch.GetElapsedTime(begin);
        await Task.Delay(2000, TestContext.Current.CancellationToken); // Wait for the ConnectAsync to complete.

        // Assert
        Assert.IsType<OperationCanceledException>(ex);
        //Assert.Equal(connectTimeout.Token, timeout.Token);
        Assert.InRange(elapsed.TotalSeconds, 0.9, 1.1);
        Assert.True(disposed);
    }

    [Fact]
    public async Task ConnectAsync_CancellationToken_Timeout_On_FirstMoveNext()
    {
        // Verify that there is no problem when the CancellationToken passed to ConnectAsync is canceled after ConnectAsync and before the first message arrives.

        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var connectTimeout = new CancellationTokenSource();
        var disposed = true;
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(
            factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance,
            onResponseHeaderAsync: metadata => Task.CompletedTask,
            onDuplexStreamingCallDisposeAction: () =>
            {
                disposed = true;
            });

        // Act
        var client = await helper.ConnectAsync(connectTimeout.Token);
        connectTimeout.Cancel();

        // Invoke Hub Method
        var t = client.Parameter_Zero().WaitAsync(timeout.Token);
        {
            // Read a hub method request payload
            var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<Nil>();
            // Write a response to the stream
            helper.WriteResponse(messageId, methodId, 123);
        }
        var result = await t;

        // Assert
        Assert.Equal(123, result);
        Assert.True(disposed);
    }
}

public interface IGreeterHubReceiver
{ }
public interface IGreeterHub : IStreamingHub<IGreeterHub, IGreeterHubReceiver>
{
    Task NoReturnValue_Parameter_Zero();
    Task<int> Parameter_Zero();
    Task<int> Parameter_One(int arg0);
    Task<int> Parameter_Two(int arg0, string arg1);
    Task<int> Parameter_Many(int arg0, string arg1, long arg2, bool arg3, byte arg4, double arg5, char arg6);
    ValueTask ValueTask_NoReturnValue_Parameter_Zero();
    ValueTask<int> ValueTask_Parameter_Zero();
    ValueTask<int> ValueTask_Parameter_One(int arg0);
    ValueTask<int> ValueTask_Parameter_Two(int arg0, string arg1);
    ValueTask<int> ValueTask_Parameter_Many(int arg0, string arg1, long arg2, bool arg3, byte arg4, double arg5, char arg6);
    void Void_Parameter_Zero();
    void Void_Parameter_One(int arg0);
    void Void_Parameter_Many(int arg0, string arg1, long arg2, bool arg3, byte arg4, double arg5, char arg6);
}
