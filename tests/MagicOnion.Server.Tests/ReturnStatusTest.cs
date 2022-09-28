using Grpc.Core;
using Xunit.Abstractions;

namespace MagicOnion.Server.Tests;

public interface IReturnStatus : IService<IReturnStatus>
{
    //// 1 is exception
    //UnaryResult<int> Unary1();
    //ClientStreamingResult<int, string> ClientStreaming1();
    //ServerStreamingResult<string> Serverstreaming1();
    //DuplexStreamingResult<int, string> DuplexStreaming1();

    //// 2 is direct return
    //UnaryResult<int> Unary2();
    //ClientStreamingResult<int, string> ClientStreaming2();
    //ServerStreamingResult<string> Serverstreaming2();
    //DuplexStreamingResult<int, string> DuplexStreaming2();
        
    // others
    UnaryResult<int> CustomThrow(int code);
}

public class ReturnStatus : ServiceBase<IReturnStatus>, IReturnStatus
{
    //public ClientStreamingResult<int, string> ClientStreaming1()
    //{
    //    throw new ReturnStatusException(Grpc.Core.StatusCode.Aborted, "a");
    //}

    //public ClientStreamingResult<int, string> ClientStreaming2()
    //{
    //    return GetClientStreamingContext<int, string>().ReturnStatus(Grpc.Core.StatusCode.InvalidArgument, "b");
    //}

    //public DuplexStreamingResult<int, string> DuplexStreaming1()
    //{
    //    throw new ReturnStatusException(Grpc.Core.StatusCode.Aborted, "a");
    //}

    //public DuplexStreamingResult<int, string> DuplexStreaming2()
    //{
    //    return GetDuplexStreamingContext<int, string>().ReturnStatus(Grpc.Core.StatusCode.InvalidArgument, "b");
    //}

    //public ServerStreamingResult<string> Serverstreaming1()
    //{
    //    throw new ReturnStatusException(Grpc.Core.StatusCode.Aborted, "a");
    //}

    //public ServerStreamingResult<string> Serverstreaming2()
    //{
    //    return GetServerStreamingContext<string>().ReturnStatus(Grpc.Core.StatusCode.InvalidArgument, "b");
    //}

    public UnaryResult<int> Unary1()
    {
        throw new ReturnStatusException(Grpc.Core.StatusCode.Aborted, "a");
    }

    public UnaryResult<int> Unary2()
    {
        return ReturnStatus<int>(Grpc.Core.StatusCode.InvalidArgument, "b");
    }

    public UnaryResult<int> CustomThrow(int code)
    {
        return ReturnStatus<int>((StatusCode)code, null);
    }
}

public class ReturnStatusTest : IClassFixture<ServerFixture<ReturnStatus>>
{
    ITestOutputHelper logger;
    IReturnStatus client;

    public ReturnStatusTest(ITestOutputHelper logger, ServerFixture<ReturnStatus> server)
    {
        this.logger = logger;
        this.client = server.CreateClient<IReturnStatus>();
    }

    //[Fact]
    //public void CheckException()
    //{
    //    Assert.Throws<RpcException>(() => client.Unary1().ResponseAsync.GetAwaiter().GetResult())
    //        .Should().Be(x => x.Status.StatusCode == StatusCode.Aborted && x.Status.Detail == "a");
    //    Assert.Throws<RpcException>(() => client.ClientStreaming1().ResponseAsync.GetAwaiter().GetResult())
    //        .Should().Be(x => x.Status.StatusCode == StatusCode.Aborted && x.Status.Detail == "a");
    //    Assert.Throws<RpcException>(() => client.Serverstreaming1().ResponseStream.MoveNext().GetAwaiter().GetResult())
    //        .Should().Be(x => x.Status.StatusCode == StatusCode.Aborted && x.Status.Detail == "a");
    //    Assert.Throws<RpcException>(() => client.DuplexStreaming1().ResponseStream.MoveNext().GetAwaiter().GetResult())
    //        .Should().Be(x => x.Status.StatusCode == StatusCode.Aborted && x.Status.Detail == "a");
    //}

    //[Fact]
    //public void CheckDirect()
    //{
    //    Assert.Throws<RpcException>(() => client.Unary2().ResponseAsync.GetAwaiter().GetResult())
    //        .Should().Be(x => x.Status.StatusCode == StatusCode.InvalidArgument && x.Status.Detail == "b");
    //    Assert.Throws<RpcException>(() => client.ClientStreaming2().ResponseAsync.GetAwaiter().GetResult())
    //        .Should().Be(x => x.Status.StatusCode == StatusCode.InvalidArgument && x.Status.Detail == "b");
    //    Assert.Throws<RpcException>(() => client.Serverstreaming2().ResponseStream.MoveNext().GetAwaiter().GetResult())
    //        .Should().Be(x => x.Status.StatusCode == StatusCode.InvalidArgument && x.Status.Detail == "b");
    //    Assert.Throws<RpcException>(() => client.DuplexStreaming2().ResponseStream.MoveNext().GetAwaiter().GetResult())
    //        .Should().Be(x => x.Status.StatusCode == StatusCode.InvalidArgument && x.Status.Detail == "b");
    //}

    [Fact]
    public void CheckCustomThrow()
    {
        var ex = Assert.Throws<RpcException>(() =>
        {
            client.CustomThrow(123).GetAwaiter().GetResult();
        });

        ex.Status.StatusCode.Should().Be((StatusCode)123);
    }
}
