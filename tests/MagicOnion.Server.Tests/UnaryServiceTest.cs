using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using MessagePack;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Server.Tests;

public class UnaryServiceTest_ReturnExceptionStackTrace : IClassFixture<MagicOnionApplicationFactory<UnaryTestService>>
{
    readonly List<string> logs;
    readonly WebApplicationFactory<Program> factory;

    public UnaryServiceTest_ReturnExceptionStackTrace(MagicOnionApplicationFactory<UnaryTestService> factory)
    {
        this.factory = factory.WithMagicOnionOptions(options =>
        {
            options.IsReturnExceptionStackTraceInErrorDetail = true;
        });

        this.logs = factory.Logs;
    }

    [Fact]
    public async Task ReturnTypeIsNilAndNonSuccessResponse()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ReturnTypeIsNilAndNonSuccessResponseAsync(StatusCode.AlreadyExists));
        ex.StatusCode.Should().Be(StatusCode.AlreadyExists);
        ex.Status.Detail.Should().Be(nameof(IUnaryTestService.ReturnTypeIsNilAndNonSuccessResponseAsync));
        logs.Should().HaveCount(1);
    }

    [Fact]
    public async Task Throw_NoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowAsync());
        ex.StatusCode.Should().Be(StatusCode.Unknown);
        logs.Should().HaveCount(1);
        ex.Message.Should().Contain("Something went wrong");
    }

    [Fact]
    public async Task Throw_OneParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowOneValueTypeParameterReturnNilAsync(1234));
        ex.StatusCode.Should().Be(StatusCode.Unknown);
        logs.Should().HaveCount(1);
        ex.Message.Should().Contain("Something went wrong");
    }

    [Fact]
    public async Task Throw_TwoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowTwoValueTypeParameterReturnNilAsync(1234, 5678));
        ex.StatusCode.Should().Be(StatusCode.Unknown);
        logs.Should().HaveCount(1);
        ex.Message.Should().Contain("Something went wrong");
    }
}

public class UnaryServiceTest : IClassFixture<MagicOnionApplicationFactory<UnaryTestService>>
{
    readonly List<string> logs;
    readonly WebApplicationFactory<Program> factory;

    public UnaryServiceTest(MagicOnionApplicationFactory<UnaryTestService> factory)
    {
        this.factory = factory;
        this.logs = factory.Logs;
    }

    [Fact]
    public async Task Throw_NoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowAsync());
        ex.StatusCode.Should().Be(StatusCode.Unknown);
        logs.Should().HaveCount(1);
        ex.Message.Should().NotContain("Something went wrong");
    }

    [Fact]
    public async Task Throw_OneParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowOneValueTypeParameterReturnNilAsync(1234));
        ex.StatusCode.Should().Be(StatusCode.Unknown);
        logs.Should().HaveCount(1);
        ex.Message.Should().NotContain("Something went wrong");
    }

    [Fact]
    public async Task Throw_TwoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient })); logs.Clear();
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowTwoValueTypeParameterReturnNilAsync(1234, 5678));
        ex.StatusCode.Should().Be(StatusCode.Unknown);
        logs.Should().HaveCount(1);
        ex.Message.Should().NotContain("Something went wrong");
    }

    [Fact]
    public async Task NoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.NoParameterReturnNilAsync()).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task NoParameterReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.NoParameterReturnValueTypeAsync()).Should().Be(1234);
    }

    [Fact]
    public async Task NoParameterReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.NoParameterReturnRefTypeAsync()).Value.Should().Be("1234");
    }

    [Fact]
    public async Task OneParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneValueTypeParameterReturnNilAsync(123)).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task OneValueTypeParameterReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneValueTypeParameterReturnValueTypeAsync(123)).Should().Be(123);
    }

    [Fact]
    public async Task OneValueTypeParameterReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneValueTypeParameterReturnRefTypeAsync(123)).Value.Should().Be("123");
    }

    [Fact]
    public async Task TwoValueTypeParametersReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoValueTypeParametersReturnNilAsync(123, 456)).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task TwoValueTypeParametersReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoValueTypeParametersReturnValueTypeAsync(123, 456)).Should().Be(123 + 456);
    }

    [Fact]
    public async Task TwoValueTypeParametersReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoValueTypeParametersReturnRefTypeAsync(123, 456)).Value.Should().Be((123 + 456).ToString());
    }

    [Fact]
    public async Task ReturnTypeIsNilAndNonSuccessResponse()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ReturnTypeIsNilAndNonSuccessResponseAsync(StatusCode.AlreadyExists));
        ex.StatusCode.Should().Be(StatusCode.AlreadyExists);
        ex.Status.Detail.Should().Be(nameof(IUnaryTestService.ReturnTypeIsNilAndNonSuccessResponseAsync));
    }

    [Fact]
    public async Task ReturnTypeIsRefTypeAndNonSuccessResponse()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ReturnTypeIsRefTypeAndNonSuccessResponseAsync(StatusCode.AlreadyExists));
        ex.StatusCode.Should().Be(StatusCode.AlreadyExists);
        ex.Status.Detail.Should().Be(nameof(IUnaryTestService.ReturnTypeIsRefTypeAndNonSuccessResponseAsync));
    }

    [Fact]
    public async Task OneRefTypeParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneRefTypeParameterReturnNilAsync(new UnaryTestMyRequest(123))).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task TwoRefTypeParametersReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoRefTypeParametersReturnNilAsync(new UnaryTestMyRequest(123), new UnaryTestMyRequest(456))).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task OneRefTypeParameterReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneRefTypeParameterReturnValueTypeAsync(new UnaryTestMyRequest(123))).Should().Be(123);
    }

    [Fact]
    public async Task TwoRefTypeParametersReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoRefTypeParametersReturnValueTypeAsync(new UnaryTestMyRequest(123), new UnaryTestMyRequest(456))).Should().Be(123 + 456);
    }

    [Fact]
    public async Task OneRefTypeParameterReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneRefTypeParameterReturnRefTypeAsync(new UnaryTestMyRequest(123))).Value.Should().Be("123");
    }

    [Fact]
    public async Task TwoRefTypeParametersReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoRefTypeParametersReturnRefTypeAsync(new UnaryTestMyRequest(123), new UnaryTestMyRequest(456))).Value.Should().Be((123 + 456).ToString());
    }

    [Fact]
    public async Task NonGeneric_NoParameter()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        await client.NonGenericNoParameterAsync();
    }

    [Fact]
    public async Task NonGeneric_OneValueTypeParameter()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        await client.NonGenericOneValueTypeParameterAsync(123);
    }

    [Fact]
    public async Task NonGeneric_TwoValueTypeParameters()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        await client.NonGenericTwoValueTypeParameterAsync(123, 456);
    }

    [Fact]
    public async Task NullResponse()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        var result = await client.NullResponseAsync(null);

        result.Should().BeNull();
    }

}
