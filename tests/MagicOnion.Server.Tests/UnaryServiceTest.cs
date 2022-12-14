using BasicServerSample.Services;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using MessagePack;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Server.Tests;

public class UnaryServiceTest : IClassFixture<WebApplicationFactory<BasicServerSample.Program>>
{
    private readonly WebApplicationFactory<BasicServerSample.Program> factory;

    public UnaryServiceTest(WebApplicationFactory<BasicServerSample.Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Throw()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ThrowAsync());
        ex.StatusCode.Should().Be(StatusCode.Unknown);
    }

    [Fact]
    public async Task NoParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.NoParameterReturnNilAsync()).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task NoParameterReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.NoParameterReturnValueTypeAsync()).Should().Be(1234);
    }

    [Fact]
    public async Task NoParameterReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.NoParameterReturnRefTypeAsync()).Value.Should().Be("1234");
    }

    [Fact]
    public async Task OneParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneValueTypeParameterReturnNilAsync(123)).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task OneValueTypeParameterReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneValueTypeParameterReturnValueTypeAsync(123)).Should().Be(123);
    }

    [Fact]
    public async Task OneValueTypeParameterReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneValueTypeParameterReturnRefTypeAsync(123)).Value.Should().Be("123");
    }

    [Fact]
    public async Task TwoValueTypeParametersReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoValueTypeParametersReturnNilAsync(123, 456)).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task TwoValueTypeParametersReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoValueTypeParametersReturnValueTypeAsync(123, 456)).Should().Be(123 + 456);
    }

    [Fact]
    public async Task TwoValueTypeParametersReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoValueTypeParametersReturnRefTypeAsync(123, 456)).Value.Should().Be((123 + 456).ToString());
    }

    [Fact]
    public async Task ReturnTypeIsNilAndNonSuccessResponse()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ReturnTypeIsNilAndNonSuccessResponseAsync(StatusCode.AlreadyExists));
        ex.StatusCode.Should().Be(StatusCode.AlreadyExists);
        ex.Status.Detail.Should().Be(nameof(IBasicUnaryService.ReturnTypeIsNilAndNonSuccessResponseAsync));
    }

    [Fact]
    public async Task ReturnTypeIsRefTypeAndNonSuccessResponse()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.ReturnTypeIsRefTypeAndNonSuccessResponseAsync(StatusCode.AlreadyExists));
        ex.StatusCode.Should().Be(StatusCode.AlreadyExists);
        ex.Status.Detail.Should().Be(nameof(IBasicUnaryService.ReturnTypeIsRefTypeAndNonSuccessResponseAsync));
    }

    [Fact]
    public async Task OneRefTypeParameterReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneRefTypeParameterReturnNilAsync(new BasicServerSample.Services.MyRequest(123))).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task TwoRefTypeParametersReturnNil()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoRefTypeParametersReturnNilAsync(new BasicServerSample.Services.MyRequest(123), new BasicServerSample.Services.MyRequest(456))).Should().Be(Nil.Default);
    }

    [Fact]
    public async Task OneRefTypeParameterReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneRefTypeParameterReturnValueTypeAsync(new BasicServerSample.Services.MyRequest(123))).Should().Be(123);
    }

    [Fact]
    public async Task TwoRefTypeParametersReturnValueType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoRefTypeParametersReturnValueTypeAsync(new BasicServerSample.Services.MyRequest(123), new BasicServerSample.Services.MyRequest(456))).Should().Be(123 + 456);
    }

    [Fact]
    public async Task OneRefTypeParameterReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.OneRefTypeParameterReturnRefTypeAsync(new BasicServerSample.Services.MyRequest(123))).Value.Should().Be("123");
    }

    [Fact]
    public async Task TwoRefTypeParametersReturnRefType()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        (await client.TwoRefTypeParametersReturnRefTypeAsync(new BasicServerSample.Services.MyRequest(123), new BasicServerSample.Services.MyRequest(456))).Value.Should().Be((123 + 456).ToString());
    }

    [Fact]
    public async Task NonGeneric_NoParameter()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        await client.NonGenericNoParameterAsync();
    }

    [Fact]
    public async Task NonGeneric_OneValueTypeParameter()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        await client.NonGenericOneValueTypeParameterAsync(123);
    }

    [Fact]
    public async Task NonGeneric_TwoValueTypeParameters()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IBasicUnaryService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        await client.NonGenericTwoValueTypeParameterAsync(123, 456);
    }
}
