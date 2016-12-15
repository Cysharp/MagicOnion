using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace MagicOnion.Tests
{
    [ZeroFormattable]
    public class MyRequest
    {
        [Index(0)]
        public virtual int Id { get; set; }
        [Index(1)]
        public virtual string Data { get; set; }
    }

    [ZeroFormattable]
    public struct MyStructRequest
    {
        [Index(0)]
        public int X;
        [Index(1)]
        public int Y;

        public MyStructRequest(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [ZeroFormattable]
    public struct MyStructResponse
    {
        [Index(0)]
        public int X;
        [Index(1)]
        public int Y;

        public MyStructResponse(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary>
    /// Represents Void/Unit.
    /// </summary>
    [ZeroFormattable]
    public struct Nil : IEquatable<Nil>
    {
        public static readonly Nil Default = default(Nil);

        public bool Equals(Nil other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }



    [ZeroFormattable]
    public class MyResponse
    {
        [Index(0)]
        public virtual int Id { get; set; }
        [Index(1)]
        public virtual string Data { get; set; }
    }


    public interface IArgumentPattern : IService<IArgumentPattern>
    {
        UnaryResult<MyResponse> Unary1(int x, int y, string z = "unknown");
        UnaryResult<MyResponse> Unary2(MyRequest req);
        UnaryResult<MyResponse> Unary3();
        UnaryResult<Nil> Unary4();
        UnaryResult<MyStructResponse> Unary5(MyStructRequest req);


        Task<ServerStreamingResult<MyResponse>> ServerStreamingResult1(int x, int y, string z = "unknown");
        Task<ServerStreamingResult<MyResponse>> ServerStreamingResult2(MyRequest req);
        Task<ServerStreamingResult<MyResponse>> ServerStreamingResult3();
        Task<ServerStreamingResult<Nil>> ServerStreamingResult4();
        Task<ServerStreamingResult<MyStructResponse>> ServerStreamingResult5(MyStructRequest req);
    }

    public class ArgumentPattern : ServiceBase<IArgumentPattern>, IArgumentPattern
    {

        public UnaryResult<MyResponse> Unary1(int x, int y, string z = "unknown")
        {
            return UnaryResult(new MyResponse
            {
                Id = x + y,
                Data = z
            });
        }

        public UnaryResult<MyResponse> Unary2(MyRequest req)
        {
            return UnaryResult(new MyResponse
            {
                Id = req.Id,
                Data = req.Data
            });
        }

        public UnaryResult<MyResponse> Unary3()
        {
            return UnaryResult(new MyResponse
            {
                Id = -1,
                Data = "NoArg"
            });
        }

        public UnaryResult<Nil> Unary4()
        {
            return UnaryResult(Nil.Default);
        }

        public UnaryResult<MyStructResponse> Unary5(MyStructRequest req)
        {
            return UnaryResult(new MyStructResponse
            {
                X = req.X,
                Y = req.Y
            });
        }


        public async Task<ServerStreamingResult<MyResponse>> ServerStreamingResult1(int x, int y, string z = "unknown")
        {
            var stream = GetServerStreamingContext<MyResponse>();
            await stream.WriteAsync(new MyResponse { Id = x + y, Data = z });
            return stream.Result();
        }

        public async Task<ServerStreamingResult<MyResponse>> ServerStreamingResult2(MyRequest req)
        {
            var stream = GetServerStreamingContext<MyResponse>();
            await stream.WriteAsync(new MyResponse { Id = req.Id, Data = req.Data });
            return stream.Result();
        }

        public async Task<ServerStreamingResult<MyResponse>> ServerStreamingResult3()
        {
            var stream = GetServerStreamingContext<MyResponse>();
            await stream.WriteAsync(new MyResponse { Id = -1, Data = "empty" });
            return stream.Result();
        }

        public async Task<ServerStreamingResult<Nil>> ServerStreamingResult4()
        {
            var stream = GetServerStreamingContext<Nil>();
            await stream.WriteAsync(Nil.Default);
            return stream.Result();
        }

        public async Task<ServerStreamingResult<MyStructResponse>> ServerStreamingResult5(MyStructRequest req)
        {
            var stream = GetServerStreamingContext<MyStructResponse>();
            await stream.WriteAsync(new MyStructResponse { X = req.X, Y = req.Y });
            return stream.Result();
        }
    }

    public class ArgumentPatternTest : IClassFixture<ServerFixture>, IDisposable
    {
        Channel channel;

        public ArgumentPatternTest(ServerFixture server)
        {
            this.channel = new Channel(server.ServerPort.Host, server.ServerPort.Port, ChannelCredentials.Insecure);
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        [Fact]
        public async Task Unary1()
        {
            {
                var client = MagicOnionClient.Create<IArgumentPattern>(channel);

                var result = await client.Unary1(10, 20, "hogehoge");

                result.Id.Is(30);
                result.Data.Is("hogehoge");
            }
            {
                var client = new ArgumentPatternManualClient(channel);

                var result = await client.Unary1(10, 20, "__omit_last_argument__");

                result.Id.Is(30);
                result.Data.Is("unknown");
            }
        }

        [Fact]
        public async Task Unary2()
        {
            var client = MagicOnionClient.Create<IArgumentPattern>(channel);

            var result = await client.Unary2(new MyRequest { Id = 30, Data = "huga" });

            result.Id.Is(30);
            result.Data.Is("huga");
        }

        [Fact]
        public async Task Unary3()
        {
            var client = MagicOnionClient.Create<IArgumentPattern>(channel);

            var result = await client.Unary3();

            result.Id.Is(-1);
            result.Data.Is("NoArg");
        }

        [Fact]
        public async Task Unary4()
        {
            var client = MagicOnionClient.Create<IArgumentPattern>(channel);

            var result = await client.Unary4();
            result.Is(Nil.Default);
        }

        [Fact]
        public async Task Unary5()
        {
            var client = MagicOnionClient.Create<IArgumentPattern>(channel);

            var result = await client.Unary5(new MyStructRequest(999, 9999));
            result.X.Is(999);
            result.Y.Is(9999);
        }

        [Fact]
        public async Task ServerStreaming()
        {
            var client = MagicOnionClient.Create<IArgumentPattern>(channel);

            {
                var callResult = await client.ServerStreamingResult1(10, 100, "aaa");
                var result = await callResult.ResponseStream.AsAsyncEnumerable().First();
                result.Id.Is(110);
                result.Data.Is("aaa");
            }

            {
                var callResult = await client.ServerStreamingResult2(new MyRequest { Id = 999, Data = "zzz" });
                var result = await callResult.ResponseStream.AsAsyncEnumerable().First();
                result.Id.Is(999);
                result.Data.Is("zzz");
            }

            {
                var callResult = await client.ServerStreamingResult3();
                var result = await callResult.ResponseStream.AsAsyncEnumerable().First();
                result.Id.Is(-1);
                result.Data.Is("empty");
            }

            {
                var callResult = await client.ServerStreamingResult4();
                var result = await callResult.ResponseStream.AsAsyncEnumerable().First();
                result.Is(Nil.Default);
            }

            {
                var callResult = await client.ServerStreamingResult5(new MyStructRequest { X = 9, Y = 100 });
                var result = await callResult.ResponseStream.AsAsyncEnumerable().First();
                result.X.Is(9);
                result.Y.Is(100);
            }
        }

        [Ignore] // client is not service.
        class ArgumentPatternManualClient : IArgumentPattern
        {
            readonly CallInvoker invoker;

            public ArgumentPatternManualClient(Channel channel)
            {
                this.invoker = new DefaultCallInvoker(channel);
            }

            public Task<ServerStreamingResult<MyResponse>> ServerStreamingResult1(int x, int y, string z = "unknown")
            {
                throw new NotImplementedException();
            }

            public Task<ServerStreamingResult<MyResponse>> ServerStreamingResult2(MyRequest req)
            {
                throw new NotImplementedException();
            }

            public Task<ServerStreamingResult<MyResponse>> ServerStreamingResult3()
            {
                throw new NotImplementedException();
            }

            public Task<ServerStreamingResult<Nil>> ServerStreamingResult4()
            {
                throw new NotImplementedException();
            }

            public Task<ServerStreamingResult<MyStructResponse>> ServerStreamingResult5(MyStructRequest req)
            {
                throw new NotImplementedException();
            }

            public UnaryResult<MyResponse> Unary1(int x, int y, string z = "unknown")
            {
                var tuple = new DynamicArgumentTuple<int, int>(x, y);
                var formatter = new DynamicArgumentTupleFormatter<DefaultResolver, int, int>(0, 0);
                var requestMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(formatter);

                var method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "Unary1", MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);

                var request = requestMarshaller.Serializer(tuple);
                var callResult = invoker.AsyncUnaryCall(method, null, default(CallOptions), request);

                var responseMarshaller = MagicOnionMarshallers.CreateZeroFormatterMarshaller(ZeroFormatter.Formatters.Formatter<DefaultResolver, MyResponse>.Default);
                return new UnaryResult<MyResponse>(callResult, responseMarshaller);
            }

            public UnaryResult<MyResponse> Unary2(MyRequest req)
            {
                throw new NotImplementedException();
            }

            public UnaryResult<MyResponse> Unary3()
            {
                throw new NotImplementedException();
            }

            public UnaryResult<Nil> Unary4()
            {
                throw new NotImplementedException();
            }

            public UnaryResult<MyStructResponse> Unary5(MyStructRequest req)
            {
                throw new NotImplementedException();
            }

            public IArgumentPattern WithCancellationToken(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public IArgumentPattern WithDeadline(DateTime deadline)
            {
                throw new NotImplementedException();
            }

            public IArgumentPattern WithHeaders(Metadata headers)
            {
                throw new NotImplementedException();
            }

            public IArgumentPattern WithHost(string host)
            {
                throw new NotImplementedException();
            }

            public IArgumentPattern WithOptions(CallOptions option)
            {
                throw new NotImplementedException();
            }
        }
    }
}
