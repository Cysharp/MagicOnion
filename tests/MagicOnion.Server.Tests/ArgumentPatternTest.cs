using Grpc.Core;
using FluentAssertions;
using MagicOnion.Client;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion.Internal;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Server.Tests;

[MessagePackObject]
public class MyRequest
{
    [Key(0)]
    public virtual int Id { get; set; }
    [Key(1)]
    public virtual string Data { get; set; }
}

[MessagePackObject]
public struct MyStructRequest
{
    [Key(0)]
    public int X;
    [Key(1)]
    public int Y;

    public MyStructRequest(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

[MessagePackObject]
public struct MyStructResponse
{
    [Key(0)]
    public int X;
    [Key(1)]
    public int Y;

    public MyStructResponse(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}


[MessagePackObject]
public class MyResponse
{
    [Key(0)]
    public virtual int Id { get; set; }
    [Key(1)]
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

public class ArgumentPatternTest : IClassFixture<ServerFixture<ArgumentPattern>>
{
    ITestOutputHelper logger;
    GrpcChannel channel;

    public ArgumentPatternTest(ITestOutputHelper logger, ServerFixture<ArgumentPattern> server)
    {
        this.logger = logger;
        this.channel = server.DefaultChannel;
    }

    [Fact]
    public async Task Unary1()
    {
        {
            var client = MagicOnionClient.Create<IArgumentPattern>(channel);

            var result = await client.Unary1(10, 20, "hogehoge");

            result.Id.Should().Be(30);
            result.Data.Should().Be("hogehoge");
        }
        {
            var client = new ArgumentPatternManualClient(channel);

            var result = await client.Unary1(10, 20, "__omit_last_argument__");

            result.Id.Should().Be(30);
            result.Data.Should().Be("unknown");
        }
    }

    [Fact]
    public async Task Unary2()
    {
        var client = MagicOnionClient.Create<IArgumentPattern>(channel);

        var result = await client.Unary2(new MyRequest { Id = 30, Data = "huga" });

        result.Id.Should().Be(30);
        result.Data.Should().Be("huga");
    }

    [Fact]
    public async Task Unary3()
    {
        var client = MagicOnionClient.Create<IArgumentPattern>(channel);

        var result = await client.Unary3();

        result.Id.Should().Be(-1);
        result.Data.Should().Be("NoArg");
    }

    [Fact]
    public async Task Unary4()
    {
        var client = MagicOnionClient.Create<IArgumentPattern>(channel);

        var result = await client.Unary4();
        result.Should().Be(Nil.Default);
    }

    [Fact]
    public async Task Unary5()
    {
        var client = MagicOnionClient.Create<IArgumentPattern>(channel);

        var result = await client.Unary5(new MyStructRequest(999, 9999));
        result.X.Should().Be(999);
        result.Y.Should().Be(9999);
    }

    async Task<T> First<T>(IAsyncStreamReader<T> reader)
    {
        await reader.MoveNext(CancellationToken.None);
        return reader.Current;
    }

    [Fact]
    public async Task ServerStreaming()
    {
        var client = MagicOnionClient.Create<IArgumentPattern>(channel);

        {
            var callResult = await client.ServerStreamingResult1(10, 100, "aaa");
            var result = await First(callResult.ResponseStream);
            result.Id.Should().Be(110);
            result.Data.Should().Be("aaa");
        }

        {
            var callResult = await client.ServerStreamingResult2(new MyRequest { Id = 999, Data = "zzz" });
            var result = await First(callResult.ResponseStream);
            result.Id.Should().Be(999);
            result.Data.Should().Be("zzz");
        }

        {
            var callResult = await client.ServerStreamingResult3();
            var result = await First(callResult.ResponseStream);
            result.Id.Should().Be(-1);
            result.Data.Should().Be("empty");
        }

        {
            var callResult = await client.ServerStreamingResult4();
            var result = await First(callResult.ResponseStream);
            result.Should().Be(Nil.Default);
        }

        {
            var callResult = await client.ServerStreamingResult5(new MyStructRequest { X = 9, Y = 100 });
            var result = await First(callResult.ResponseStream);
            result.X.Should().Be(9);
            result.Y.Should().Be(100);
        }
    }

    [Ignore] // client is not service.
    class ArgumentPatternManualClient : IArgumentPattern
    {
        readonly CallInvoker invoker;

        public ArgumentPatternManualClient(GrpcChannel channel)
        {
            this.invoker = channel.CreateCallInvoker();
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

            var method = GrpcMethodHelper.CreateMethod<DynamicArgumentTuple<int, int>, MyResponse, Box<DynamicArgumentTuple<int, int>>, MyResponse>(MethodType.Unary, "IArgumentPattern", "Unary1", MessagePackSerializerOptions.Standard);
            var request = Box.Create(tuple);

            var callResult = invoker.AsyncUnaryCall(method.Method, null, default(CallOptions), request);

            var response = new ResponseContext<MyResponse>(callResult);
            return new UnaryResult<MyResponse>(Task.FromResult<IResponseContext<MyResponse>>(response));
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