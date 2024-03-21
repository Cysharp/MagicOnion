#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using MessagePack;
using Grpc.Net.Client;
using Xunit.Abstractions;

namespace MagicOnion.Server.Tests;

public interface IMessageReceiver
{
    void VoidOnConnected(int x, string y, double z);
    //Task ZeroArgument();
    //Task OneArgument(int x);
    //Task MoreArgument(int x, string y, double z);
    void VoidZeroArgument();
    void VoidOneArgument(int x);
    void VoidMoreArgument(int x, string y, double z);
    //Task OneArgument2(TestObject x);
    void VoidOneArgument2(TestObject x);
    //Task OneArgument3(TestObject[] x);
    void VoidOneArgument3(TestObject[] x);
}

public interface ITestHub : IStreamingHub<ITestHub, IMessageReceiver>
{
    Task ZeroArgument();
    Task OneArgument(int x);
    Task MoreArgument(int x, string y, double z);

    Task<int> RetrunZeroArgument();
    Task<string> RetrunOneArgument(int x);
    Task<double> RetrunMoreArgument(int x, string y, double z);

    Task OneArgument2(TestObject x);
    Task<TestObject> RetrunOneArgument2(TestObject x);

    Task OneArgument3(TestObject[] x);
    Task<TestObject[]> RetrunOneArgument3(TestObject[] x);
}

[MessagePackObject]
public class TestObject
{
    [Key(0)]
    public int X { get; set; }
    [Key(1)]
    public int Y { get; set; }
    [Key(2)]
    public int Z { get; set; }
}

public class TestHub : StreamingHubBase<ITestHub, IMessageReceiver>, ITestHub
{
    IGroup group;

    protected override async ValueTask OnConnecting()
    {
        group = await Group.AddAsync("global");
    }

    protected override async ValueTask OnConnected()
    {
        BroadcastToSelf(group).VoidOnConnected(123, "foo", 12.3f);
    }

    protected override async ValueTask OnDisconnected()
    {
        if (group != null) await group.RemoveAsync(Context);
    }


    public async Task MoreArgument(int x, string y, double z)
    {
        BroadcastToSelf(group).VoidMoreArgument(x, y, z);
        //await Broadcast(group).MoreArgument(x, y, z);
    }

    public async Task OneArgument(int x)
    {
        Broadcast(group).VoidOneArgument(x);
        //            await Broadcast(group).OneArgument(x);
    }

    public async Task OneArgument2(TestObject x)
    {
        Broadcast(group).VoidOneArgument2(x);
        //await Broadcast(group).OneArgument2(x);
    }

    public async Task OneArgument3(TestObject[] x)
    {
        Broadcast(group).VoidOneArgument3(x);
        //await Broadcast(group).OneArgument3(x);
    }

    public Task<double> RetrunMoreArgument(int x, string y, double z)
    {
        return Task.FromResult(z);
    }

    public async Task<string> RetrunOneArgument(int x)
    {
        return x.ToString();
    }

    public async Task<TestObject> RetrunOneArgument2(TestObject x)
    {
        return x;
    }

    public async Task<TestObject[]> RetrunOneArgument3(TestObject[] x)
    {
        return x;
    }

    public async Task<int> RetrunZeroArgument()
    {
        return 1000;
    }

    public async Task ZeroArgument()
    {
        Broadcast(group).VoidZeroArgument();
        //await Broadcast(group).ZeroArgument();
    }
}

public class BasicStreamingHubTest : IMessageReceiver, IDisposable, IClassFixture<ServerFixture<TestHub>>
{
    ITestOutputHelper logger;
    GrpcChannel channel;
    ITestHub client;

    public BasicStreamingHubTest(ITestOutputHelper logger, ServerFixture<TestHub> server)
    {
        this.logger = logger;
        this.channel = server.DefaultChannel;
    }

    [Fact]
    public async Task OnConnected()
    {
        client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        var x = await voidOnConnectedTask.Task;
        x.Should().Be((123, "foo", 12.3f));
        await client.DisposeAsync();
    }

    [Fact]
    public async Task ZeroArgument()
    {
        try
        {
            client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
            await client.ZeroArgument();
            await voidZeroTask.Task;
            //await zeroTask.Task;
            // ok, pass.

            await client.DisposeAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    [Fact]
    public async Task OneArgument()
    {
        var client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        await client.OneArgument(100);
        //var x = await oneTask.Task;
        var y = await voidoneTask.Task;
        //x.Should().Be(100);
        y.Should().Be(100);
        await client.DisposeAsync();
    }

    [Fact]
    public async Task MoreArgument()
    {
        var client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        await client.MoreArgument(100, "foo", 10.3);
        //var x = await moreTask.Task;
        var y = await voidmoreTask.Task;
        //x.Should().Be((100, "foo", 10.3));
        y.Should().Be((100, "foo", 10.3));
        await client.DisposeAsync();
    }

    [Fact]
    public async Task RetrunZeroArgument()
    {
        var client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        var v = await client.RetrunZeroArgument();
        v.Should().Be(1000);
        await client.DisposeAsync();
    }
    [Fact]
    public async Task RetrunOneArgument()
    {
        var client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        var v = await client.RetrunZeroArgument();
        v.Should().Be(1000);
        await client.DisposeAsync();
    }
    [Fact]
    public async Task RetrunMoreArgument()
    {
        var client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        var v = await client.RetrunMoreArgument(10, "foo", 30.4);
        v.Should().Be(30.4);
        await client.DisposeAsync();
    }

    [Fact]
    public async Task OneArgument2()
    {
        var client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        await client.OneArgument2(new TestObject() { X = 10, Y = 99, Z = 100 });
        {
            //var v = await one2Task.Task;
            //v.X.Should().Be(10);
            //v.Y.Should().Be(99);
            //v.Z.Should().Be(100);
        }
        {
            var v = await voidone2Task.Task;
            v.X.Should().Be(10);
            v.Y.Should().Be(99);
            v.Z.Should().Be(100);
        }
        await client.DisposeAsync();
    }
    [Fact]
    public async Task RetrunOneArgument2()
    {
        var client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        var v = await client.RetrunOneArgument2(new TestObject() { X = 10, Y = 99, Z = 100 });
        v.X.Should().Be(10);
        v.Y.Should().Be(99);
        v.Z.Should().Be(100);
        await client.DisposeAsync();
    }

    [Fact]
    public async Task OneArgument3()
    {
        var client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        await client.OneArgument3(new[]
        {
            new TestObject() { X = 10, Y = 99, Z = 100 },
            new TestObject() { X = 5, Y = 39, Z = 200 },
            new TestObject() { X = 4, Y = 59, Z = 300 },
        });
        {
            //var v = await one3Task.Task;

            //v[0].X.Should().Be(10);
            //v[0].Y.Should().Be(99);
            //v[0].Z.Should().Be(100);

            //v[1].X.Should().Be(5);
            //v[1].Y.Should().Be(39);
            //v[1].Z.Should().Be(200);

            //v[2].X.Should().Be(4);
            //v[2].Y.Should().Be(59);
            //v[2].Z.Should().Be(300);
        }
        {
            var v = await voidone3Task.Task;

            v[0].X.Should().Be(10);
            v[0].Y.Should().Be(99);
            v[0].Z.Should().Be(100);

            v[1].X.Should().Be(5);
            v[1].Y.Should().Be(39);
            v[1].Z.Should().Be(200);

            v[2].X.Should().Be(4);
            v[2].Y.Should().Be(59);
            v[2].Z.Should().Be(300);
        }
        await client.DisposeAsync();
    }
    [Fact]
    public async Task RetrunOneArgument3()
    {
        var client = await StreamingHubClient.ConnectAsync<ITestHub, IMessageReceiver>(channel, this);
        var v = await client.RetrunOneArgument3(new[]
        {
            new TestObject() { X = 10, Y = 99, Z = 100 },
            new TestObject() { X = 5, Y = 39, Z = 200 },
            new TestObject() { X = 4, Y = 59, Z = 300 },
        });

        v[0].X.Should().Be(10);
        v[0].Y.Should().Be(99);
        v[0].Z.Should().Be(100);

        v[1].X.Should().Be(5);
        v[1].Y.Should().Be(39);
        v[1].Z.Should().Be(200);

        v[2].X.Should().Be(4);
        v[2].Y.Should().Be(59);
        v[2].Z.Should().Be(300);
        await client.DisposeAsync();
    }



    //TaskCompletionSource<(int, string, double)> moreTask = new TaskCompletionSource<(int, string, double)>();
    //async Task IMessageReceiver.MoreArgument(int x, string y, double z)
    //{
    //    moreTask.TrySetResult((x, y, z));
    //}

    //TaskCompletionSource<int> oneTask = new TaskCompletionSource<int>();
    //async Task IMessageReceiver.OneArgument(int x)
    //{
    //    oneTask.TrySetResult(x);
    //}

    //TaskCompletionSource<TestObject> one2Task = new TaskCompletionSource<TestObject>();
    //async Task IMessageReceiver.OneArgument2(TestObject x)
    //{
    //    one2Task.TrySetResult(x);
    //}

    //TaskCompletionSource<TestObject[]> one3Task = new TaskCompletionSource<TestObject[]>();
    //async Task IMessageReceiver.OneArgument3(TestObject[] x)
    //{
    //    one3Task.TrySetResult(x);
    //}

    TaskCompletionSource<(int, string, double)> voidOnConnectedTask = new TaskCompletionSource<(int, string, double)>();
    void IMessageReceiver.VoidOnConnected(int x, string y, double z)
    {
        voidOnConnectedTask.TrySetResult((x, y, z));
    }


    TaskCompletionSource<(int, string, double)> voidmoreTask = new TaskCompletionSource<(int, string, double)>();
    void IMessageReceiver.VoidMoreArgument(int x, string y, double z)
    {
        voidmoreTask.TrySetResult((x, y, z));
    }

    TaskCompletionSource<int> voidoneTask = new TaskCompletionSource<int>();
    void IMessageReceiver.VoidOneArgument(int x)
    {
        voidoneTask.TrySetResult(x);
    }

    TaskCompletionSource<TestObject> voidone2Task = new TaskCompletionSource<TestObject>();
    void IMessageReceiver.VoidOneArgument2(TestObject x)
    {
        voidone2Task.TrySetResult(x);
    }

    TaskCompletionSource<TestObject[]> voidone3Task = new TaskCompletionSource<TestObject[]>();
    void IMessageReceiver.VoidOneArgument3(TestObject[] x)
    {
        voidone3Task.TrySetResult(x);
    }

    TaskCompletionSource<object> voidZeroTask = new TaskCompletionSource<object>();
    void IMessageReceiver.VoidZeroArgument()
    {
        voidZeroTask.TrySetResult(null);
    }

    //TaskCompletionSource<object> zeroTask = new TaskCompletionSource<object>();
    //async Task IMessageReceiver.ZeroArgument()
    //{
    //    zeroTask.TrySetResult(null);
    //}

    public void Dispose()
    {
        if (client != null)
        {
            client.DisposeAsync().Wait();
        }
    }
}


public interface IEmptyReceiver
{
}

public interface IMoreCheckHub : IStreamingHub<IMoreCheckHub, IEmptyReceiver>
{
    Task ReceiveExceptionAsync();
    Task StatusCodeAsync();
    Task FilterCheckAsync();
}

public class MoreCheckHub : StreamingHubBase<IMoreCheckHub, IEmptyReceiver>, IMoreCheckHub
{
    public Task ReceiveExceptionAsync()
    {
        throw new Exception("foo");
    }

    public async Task StatusCodeAsync()
    {
        throw new ReturnStatusException((StatusCode)99, "foo bar baz");
    }

    [StreamingHubTestFilter]
    public async Task FilterCheckAsync()
    {

    }
}

public class StreamingHubTestFilterAttribute : StreamingHubFilterAttribute
{
    public static bool CalledBefore;
    public static bool CalledAfter;

    public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
    {
        context.Items["HubFilter1_AF"] = "BeforeOK";
        await next.Invoke(context);
        context.Items["HubFilter1_BF"] = "AfterOK";
    }
}

public class MoreCheckHubTest : IEmptyReceiver, IDisposable, IClassFixture<ServerFixture<MoreCheckHub>>
{
    ITestOutputHelper logger;
    GrpcChannel channel;
    IMoreCheckHub client;

    public MoreCheckHubTest(ITestOutputHelper logger, ServerFixture<MoreCheckHub> server)
    {
        this.logger = logger;
        this.channel = server.DefaultChannel;
    }

    [Fact]
    public async Task ReceiveEx()
    {
        client = await StreamingHubClient.ConnectAsync<IMoreCheckHub, IEmptyReceiver>(channel, this);

        var ex = Assert.Throws<RpcException>(() =>
        {
            client.ReceiveExceptionAsync().GetAwaiter().GetResult();
        });

        ex.StatusCode.Should().Be(StatusCode.Internal);
        logger.WriteLine(ex.ToString());

        await client.DisposeAsync();
    }

    [Fact]
    public async Task StatusCodeEx()
    {
        client = await StreamingHubClient.ConnectAsync<IMoreCheckHub, IEmptyReceiver>(channel, this);

        var ex = Assert.Throws<RpcException>(() =>
        {
            client.StatusCodeAsync().GetAwaiter().GetResult();
        });

        ex.StatusCode.Should().Be((StatusCode)99);
        logger.WriteLine(ex.Status.Detail);
        logger.WriteLine(ex.ToString());

        await client.DisposeAsync();
    }

    [Fact]
    public async Task Filter()
    {
        client = await StreamingHubClient.ConnectAsync<IMoreCheckHub, IEmptyReceiver>(channel, this);
        await client.FilterCheckAsync();
    }


    public void Dispose()
    {
        if (client != null)
        {
            client.DisposeAsync().Wait();
        }
    }
}


#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
