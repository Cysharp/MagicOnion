using MagicOnion.Client.DynamicClient;
using Microsoft.Extensions.Time.Testing;
using System.Buffers;

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
    public async Task Heartbeat_Interval()
    {
        // Arrange
        var origin = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(origin);
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var options = StreamingHubClientOptions.CreateWithDefault().WithClientHeartbeatInterval(TimeSpan.FromMilliseconds(100)).WithTimeProvider(timeProvider);
        var client = await helper.ConnectAsync(options, timeout.Token);
        byte[] sentAt = [0xcf, 0x00, 0x00, 0x01, 0x90, 0x6b, 0x97, 0x5c, 0x00]; // MessagePackWriter.Write(timeProvider.GetUtcNow().ToUnixTimeMilliseconds());

        // Act
        var t = client.Parameter_One(1234);
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(100); // Wait for processing queue.
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(100); // Wait for processing queue.
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        await Task.Delay(100); // Wait for processing queue.

        // Assert
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<int>();
        Assert.Equal(1234, requestBody);

        var request1 = await helper.ReadRequestRawAsync();
        var request2 = await helper.ReadRequestRawAsync();
        var request3 = await helper.ReadRequestRawAsync();
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0xc0 /* Nil */, 0xc0 /* Nil */, 0x91 /* Array(1) */, .. (ToMessagePackBytes(origin.AddMilliseconds(100)))], request1.ToArray());
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0xc0 /* Nil */, 0xc0 /* Nil */, 0x91 /* Array(1) */, .. (ToMessagePackBytes(origin.AddMilliseconds(200)))], request2.ToArray());
        Assert.Equal((byte[])[0x94 /* Array(4) */, 0x7e /* 0x7e(127) */, 0xc0 /* Nil */, 0xc0 /* Nil */, 0x91 /* Array(1) */, .. (ToMessagePackBytes(origin.AddMilliseconds(300)))], request3.ToArray());

        static byte[] ToMessagePackBytes(DateTimeOffset dt)
        {
            var ms = dt.ToUnixTimeMilliseconds();

            var arrayBufferWriter = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(arrayBufferWriter);
            writer.Write(ms);
            writer.Flush();
            return arrayBufferWriter.WrittenMemory.ToArray();
        }
    }

    [Fact]
    public async Task Heartbeat_Respond()
    {
        // Arrange
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var options = StreamingHubClientOptions.CreateWithDefault().WithClientHeartbeatInterval(Timeout.InfiniteTimeSpan); // Disable Heartbeat timer.
        var client = await helper.ConnectAsync(options, timeout.Token);

        // Act
        var t = client.Parameter_One(1234);
        helper.WriteResponseRaw([0x95 /* Array(5) */, 0x7f /* Type:127 */, 0xc0, 0xc0, 0xc0, 0xc0 /* Extra */]); // Simulate heartbeat from the server.
        await Task.Delay(100);

        // Assert
        var (messageId, methodId, requestBody) = await helper.ReadRequestAsync<int>();
        Assert.Equal(1234, requestBody);

        var request1 = await helper.ReadRequestRawAsync();
        Assert.Equal([0x94, 0x7f, 0xc0, 0xc0, 0xc0], request1.ToArray()); // Respond to the heartbeat from the server.
    }

    [Fact]
    public async Task Heartbeat_Extra()
    {
        // Arrange
        var received = Array.Empty<byte>();
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IGreeterHub, IGreeterHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var options = StreamingHubClientOptions.CreateWithDefault()
            .WithServerHeartbeatReceived(x => received = x.ToArray())
            .WithClientHeartbeatInterval(Timeout.InfiniteTimeSpan); // Disable Heartbeat timer.
        var client = await helper.ConnectAsync(options, timeout.Token);

        // Act
        helper.WriteResponseRaw([0x95 /* Array(5) */, 0x7f /* Type:127 */, 0xc0, 0xc0, 0xc0, .."Hello World"u8 /* Extra */]); // Simulate heartbeat from the server.
        await Task.Delay(100);

        // Assert
        Assert.Equal([.. "Hello World"u8], received); // Respond to the heartbeat from the server.
        var request1 = await helper.ReadRequestRawAsync();
        Assert.Equal([0x94, 0x7f, 0xc0, 0xc0, 0xc0], request1.ToArray()); // Respond to the heartbeat from the server.
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
