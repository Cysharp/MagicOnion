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
        Assert.Equal(StatusCode.AlreadyExists, ex.StatusCode);
        Assert.Equal(nameof(IUnaryTestService.ReturnTypeIsNilAndNonSuccessResponseAsync), ex.Status.Detail);
        Assert.Equal(1, logs.Count());
    }

    [Fact]
    public async Task Throw_NoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowAsync());
        Assert.Equal(StatusCode.Unknown, ex.StatusCode);
        Assert.Equal(1, logs.Count());
        Assert.Contains("Something went wrong", ex.Message);
    }

    [Fact]
    public async Task Throw_OneParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowOneValueTypeParameterReturnNilAsync(1234));
        Assert.Equal(StatusCode.Unknown, ex.StatusCode);
        Assert.Equal(1, logs.Count());
        Assert.Contains("Something went wrong", ex.Message);
    }

    [Fact]
    public async Task Throw_TwoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowTwoValueTypeParameterReturnNilAsync(1234, 5678));
        Assert.Equal(StatusCode.Unknown, ex.StatusCode);
        Assert.Equal(1, logs.Count());
        Assert.Contains("Something went wrong", ex.Message);
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
        Assert.Equal(StatusCode.Unknown, ex.StatusCode);
        Assert.Equal(1, logs.Count());
        Assert.DoesNotContain("Something went wrong", ex.Message);
    }

    [Fact]
    public async Task Throw_OneParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowOneValueTypeParameterReturnNilAsync(1234));
        Assert.Equal(StatusCode.Unknown, ex.StatusCode);
        Assert.Equal(1, logs.Count());
        Assert.DoesNotContain("Something went wrong", ex.Message);
    }

    [Fact]
    public async Task Throw_TwoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient })); logs.Clear();
        logs.Clear();

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowTwoValueTypeParameterReturnNilAsync(1234, 5678));
        Assert.Equal(StatusCode.Unknown, ex.StatusCode);
        Assert.Equal(1, logs.Count());
        Assert.DoesNotContain("Something went wrong", ex.Message);
    }

    [Fact]
    public async Task NoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(Nil.Default, (await client.NoParameterReturnNilAsync()));
    }

    [Fact]
    public async Task NoParameterReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(1234, (await client.NoParameterReturnValueTypeAsync()));
    }

    [Fact]
    public async Task NoParameterReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal("1234", (await client.NoParameterReturnRefTypeAsync()).Value);
    }

    [Fact]
    public async Task OneParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(Nil.Default, (await client.OneValueTypeParameterReturnNilAsync(123)));
    }

    [Fact]
    public async Task OneValueTypeParameterReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(123, (await client.OneValueTypeParameterReturnValueTypeAsync(123)));
    }

    [Fact]
    public async Task OneValueTypeParameterReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal("123", (await client.OneValueTypeParameterReturnRefTypeAsync(123)).Value);
    }

    [Fact]
    public async Task TwoValueTypeParametersReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(Nil.Default, (await client.TwoValueTypeParametersReturnNilAsync(123, 456)));
    }

    [Fact]
    public async Task TwoValueTypeParametersReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(123 + 456, (await client.TwoValueTypeParametersReturnValueTypeAsync(123, 456)));
    }

    [Fact]
    public async Task TwoValueTypeParametersReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal((123 + 456).ToString(), (await client.TwoValueTypeParametersReturnRefTypeAsync(123, 456)).Value);
    }

    [Fact]
    public async Task ReturnTypeIsNilAndNonSuccessResponse()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ReturnTypeIsNilAndNonSuccessResponseAsync(StatusCode.AlreadyExists));
        Assert.Equal(StatusCode.AlreadyExists, ex.StatusCode);
        Assert.Equal(nameof(IUnaryTestService.ReturnTypeIsNilAndNonSuccessResponseAsync), ex.Status.Detail);
    }

    [Fact]
    public async Task ReturnTypeIsRefTypeAndNonSuccessResponse()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ReturnTypeIsRefTypeAndNonSuccessResponseAsync(StatusCode.AlreadyExists));
        Assert.Equal(StatusCode.AlreadyExists, ex.StatusCode);
        Assert.Equal(nameof(IUnaryTestService.ReturnTypeIsRefTypeAndNonSuccessResponseAsync), ex.Status.Detail);
    }

    [Fact]
    public async Task OneRefTypeParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(Nil.Default, (await client.OneRefTypeParameterReturnNilAsync(new UnaryTestMyRequest(123))));
    }

    [Fact]
    public async Task TwoRefTypeParametersReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(Nil.Default, (await client.TwoRefTypeParametersReturnNilAsync(new UnaryTestMyRequest(123), new UnaryTestMyRequest(456))));
    }

    [Fact]
    public async Task OneRefTypeParameterReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(123, (await client.OneRefTypeParameterReturnValueTypeAsync(new UnaryTestMyRequest(123))));
    }

    [Fact]
    public async Task TwoRefTypeParametersReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal(123 + 456, (await client.TwoRefTypeParametersReturnValueTypeAsync(new UnaryTestMyRequest(123), new UnaryTestMyRequest(456))));
    }

    [Fact]
    public async Task OneRefTypeParameterReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal("123", (await client.OneRefTypeParameterReturnRefTypeAsync(new UnaryTestMyRequest(123))).Value);
    }

    [Fact]
    public async Task TwoRefTypeParametersReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IUnaryTestService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        Assert.Equal((123 + 456).ToString(), (await client.TwoRefTypeParametersReturnRefTypeAsync(new UnaryTestMyRequest(123), new UnaryTestMyRequest(456))).Value);
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

        Assert.Null(result);
    }

}
