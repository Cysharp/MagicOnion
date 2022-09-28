using System.Net.Http.Headers;
using AuthSample;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Server.Tests;

public class AuthorizeStreamingHubTest : IClassFixture<WebApplicationFactory<Startup>>, IAuthorizeHubReceiver
{
    private readonly WebApplicationFactory<Startup> _factory;

    public AuthorizeStreamingHubTest(WebApplicationFactory<Startup> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Authorize_Connect()
    {
        var httpClient = _factory.CreateDefaultClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Alice");
        var client = await StreamingHubClient.ConnectAsync<IAuthorizeHub, IAuthorizeHubReceiver>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }), this);
        var userName = await client.GetUserNameAsync();
        userName.Should().Be("Alice");
    }

    [Fact]
    public async Task Unauthenticated_Connect()
    {
        var httpClient = _factory.CreateDefaultClient();

        var ex = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            var client = await StreamingHubClient.ConnectAsync<IAuthorizeHub, IAuthorizeHubReceiver>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }), this);
        });

        ex.StatusCode.Should().Be(StatusCode.Unauthenticated);
    }
}
