#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Tests
{
    public interface IMessageReceiver
    {
        Task ZeroArgument();
        Task OneArgument(int x);
        Task MoreArgument(int x, string y, double z);
        void VoidZeroArgument();
        void VoidOneArgument(int x);
        void VoidMoreArgument(int x, string y, double z);
        Task OneArgument2(TestObject x);
        void VoidOneArgument2(TestObject x);
        Task OneArgument3(TestObject[] x);
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

        protected override async ValueTask OnDisconnected()
        {
            if (group != null) await group.RemoveAsync(Context);
        }


        public async Task MoreArgument(int x, string y, double z)
        {
            Broadcast(group).VoidMoreArgument(x, y, z);
            await Broadcast(group).MoreArgument(x, y, z);
        }

        public async Task OneArgument(int x)
        {
            Broadcast(group).VoidOneArgument(x);
            await Broadcast(group).OneArgument(x);
        }

        public async Task OneArgument2(TestObject x)
        {
            Broadcast(group).VoidOneArgument2(x);
            await Broadcast(group).OneArgument2(x);
        }

        public async Task OneArgument3(TestObject[] x)
        {
            Broadcast(group).VoidOneArgument3(x);
            await Broadcast(group).OneArgument3(x);
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
            await Broadcast(group).ZeroArgument();
        }
    }

    [Collection(nameof(AllAssemblyGrpcServerFixture))]
    public class BasicStreamingHubTest : IMessageReceiver, IDisposable
    {
        ITestOutputHelper logger;
        Channel channel;
        ITestHub client;

        public BasicStreamingHubTest(ITestOutputHelper logger, ServerFixture server)
        {
            this.logger = logger;
            this.channel = server.DefaultChannel;
        }

        [Fact]
        public async Task ZeroArgument()
        {
            client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            await client.ZeroArgument();
            await voidZeroTask.Task;
            await zeroTask.Task;
            // ok, pass.

            await client.DisposeAsync();
        }

        [Fact]
        public async Task OneArgument()
        {
            var client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            await client.OneArgument(100);
            var x = await oneTask.Task;
            var y = await voidoneTask.Task;
            x.Is(100);
            y.Is(100);
            await client.DisposeAsync();
        }

        [Fact]
        public async Task MoreArgument()
        {
            var client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            await client.MoreArgument(100, "foo", 10.3);
            var x = await moreTask.Task;
            var y = await voidmoreTask.Task;
            x.Is((100, "foo", 10.3));
            y.Is((100, "foo", 10.3));
            await client.DisposeAsync();
        }

        [Fact]
        public async Task RetrunZeroArgument()
        {
            var client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            var v = await client.RetrunZeroArgument();
            v.Is(1000);
            await client.DisposeAsync();
        }
        [Fact]
        public async Task RetrunOneArgument()
        {
            var client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            var v = await client.RetrunZeroArgument();
            v.Is(1000);
            await client.DisposeAsync();
        }
        [Fact]
        public async Task RetrunMoreArgument()
        {
            var client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            var v = await client.RetrunMoreArgument(10, "foo", 30.4);
            v.Is(30.4);
            await client.DisposeAsync();
        }

        [Fact]
        public async Task OneArgument2()
        {
            var client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            await client.OneArgument2(new TestObject() { X = 10, Y = 99, Z = 100 });
            {
                var v = await one2Task.Task;
                v.X.Is(10);
                v.Y.Is(99);
                v.Z.Is(100);
            }
            {
                var v = await voidone2Task.Task;
                v.X.Is(10);
                v.Y.Is(99);
                v.Z.Is(100);
            }
            await client.DisposeAsync();
        }
        [Fact]
        public async Task RetrunOneArgument2()
        {
            var client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            var v = await client.RetrunOneArgument2(new TestObject() { X = 10, Y = 99, Z = 100 });
            v.X.Is(10);
            v.Y.Is(99);
            v.Z.Is(100);
            await client.DisposeAsync();
        }

        [Fact]
        public async Task OneArgument3()
        {
            var client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            await client.OneArgument3(new[]
            {
                new TestObject() { X = 10, Y = 99, Z = 100 },
                new TestObject() { X = 5, Y = 39, Z = 200 },
                new TestObject() { X = 4, Y = 59, Z = 300 },
            });
            {
                var v = await one3Task.Task;

                v[0].X.Is(10);
                v[0].Y.Is(99);
                v[0].Z.Is(100);

                v[1].X.Is(5);
                v[1].Y.Is(39);
                v[1].Z.Is(200);

                v[2].X.Is(4);
                v[2].Y.Is(59);
                v[2].Z.Is(300);
            }
            {
                var v = await voidone3Task.Task;

                v[0].X.Is(10);
                v[0].Y.Is(99);
                v[0].Z.Is(100);

                v[1].X.Is(5);
                v[1].Y.Is(39);
                v[1].Z.Is(200);

                v[2].X.Is(4);
                v[2].Y.Is(59);
                v[2].Z.Is(300);
            }
            await client.DisposeAsync();
        }
        [Fact]
        public async Task RetrunOneArgument3()
        {
            var client = StreamingHubClient.Connect<ITestHub, IMessageReceiver>(channel, this);
            var v = await client.RetrunOneArgument3(new[]
            {
                new TestObject() { X = 10, Y = 99, Z = 100 },
                new TestObject() { X = 5, Y = 39, Z = 200 },
                new TestObject() { X = 4, Y = 59, Z = 300 },
            });

            v[0].X.Is(10);
            v[0].Y.Is(99);
            v[0].Z.Is(100);

            v[1].X.Is(5);
            v[1].Y.Is(39);
            v[1].Z.Is(200);

            v[2].X.Is(4);
            v[2].Y.Is(59);
            v[2].Z.Is(300);
            await client.DisposeAsync();
        }



        TaskCompletionSource<(int, string, double)> moreTask = new TaskCompletionSource<(int, string, double)>();
        async Task IMessageReceiver.MoreArgument(int x, string y, double z)
        {
            moreTask.TrySetResult((x, y, z));
        }

        TaskCompletionSource<int> oneTask = new TaskCompletionSource<int>();
        async Task IMessageReceiver.OneArgument(int x)
        {
            oneTask.TrySetResult(x);
        }

        TaskCompletionSource<TestObject> one2Task = new TaskCompletionSource<TestObject>();
        async Task IMessageReceiver.OneArgument2(TestObject x)
        {
            one2Task.TrySetResult(x);
        }

        TaskCompletionSource<TestObject[]> one3Task = new TaskCompletionSource<TestObject[]>();
        async Task IMessageReceiver.OneArgument3(TestObject[] x)
        {
            one3Task.TrySetResult(x);
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

        TaskCompletionSource<object> zeroTask = new TaskCompletionSource<object>();
        async Task IMessageReceiver.ZeroArgument()
        {
            zeroTask.TrySetResult(null);
        }

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
        public static bool calledBefore;
        public static bool calledAfter;

        public StreamingHubTestFilterAttribute(Func<StreamingHubContext, ValueTask> next) : base(next) { }
        public StreamingHubTestFilterAttribute() : base(null) { }

        public override async ValueTask Invoke(StreamingHubContext context)
        {
            context.Items["HubFilter1_AF"] = "BeforeOK";
            await Next.Invoke(context);
            context.Items["HubFilter1_BF"] = "AfterOK";
        }
    }


    [Collection(nameof(AllAssemblyGrpcServerFixture))]
    public class MoreCheckHubTest : IEmptyReceiver, IDisposable
    {
        ITestOutputHelper logger;
        Channel channel;
        IMoreCheckHub client;

        public MoreCheckHubTest(ITestOutputHelper logger, ServerFixture server)
        {
            this.logger = logger;
            this.channel = server.DefaultChannel;
        }

        [Fact]
        public async Task ReceiveEx()
        {
            client = StreamingHubClient.Connect<IMoreCheckHub, IEmptyReceiver>(channel, this);

            var ex = Assert.Throws<RpcException>(() =>
            {
                client.ReceiveExceptionAsync().GetAwaiter().GetResult();
            });

            ex.StatusCode.Is(StatusCode.Internal);
            logger.WriteLine(ex.ToString());

            await client.DisposeAsync();
        }

        [Fact]
        public async Task StatusCodeEx()
        {
            client = StreamingHubClient.Connect<IMoreCheckHub, IEmptyReceiver>(channel, this);

            var ex = Assert.Throws<RpcException>(() =>
            {
                client.StatusCodeAsync().GetAwaiter().GetResult();
            });

            ex.StatusCode.Is((StatusCode)99);
            logger.WriteLine(ex.Status.Detail);
            logger.WriteLine(ex.ToString());

            await client.DisposeAsync();
        }

        [Fact]
        public async Task Filter()
        {
            client = StreamingHubClient.Connect<IMoreCheckHub, IEmptyReceiver>(channel, this);
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
}


#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
