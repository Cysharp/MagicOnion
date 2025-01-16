using System.Net.Http.Headers;
using AuthSample;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Server.Tests;

public class AuthorizeServiceTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> factory;

    public AuthorizeServiceTest(WebApplicationFactory<Startup> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Class_Authorize()
    {
        var httpClient = factory.CreateDefaultClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Alice");
        var client = MagicOnionClient.Create<IAuthorizeClassService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        var userName = await client.GetUserNameAsync();
        Assert.Equal("Alice", userName);
    }

    [Fact]
    public async Task Class_Unauthorized()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IAuthorizeClassService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.GetUserNameAsync());
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task Class_AllowAnonymous()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IAuthorizeClassService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        Assert.Equal(579, (await client.AddAsync(123, 456)));
    }

    [Fact]
    public async Task Method_Authorize()
    {
        var httpClient = factory.CreateDefaultClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Alice");
        var client = MagicOnionClient.Create<IAuthorizeMethodService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        var userName = await client.GetUserNameAsync();
        Assert.Equal("Alice", userName);
    }

    [Fact]
    public async Task Method_Unauthorized()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IAuthorizeMethodService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.GetUserNameAsync());
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task Method_AllowAnonymous()
    {
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IAuthorizeMethodService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        Assert.Equal(579, (await client.AddAsync(123, 456)));
    }
}
