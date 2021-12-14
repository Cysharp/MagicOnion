using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AuthSample;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Server.Tests.Tests
{
    public class AuthorizeStreamingHubTest : IClassFixture<WebApplicationFactory<AuthSample.Startup>>, IAuthorizeHubReceiver
    {
        private readonly WebApplicationFactory<AuthSample.Startup> _factory;

        public AuthorizeStreamingHubTest(WebApplicationFactory<AuthSample.Startup> factory)
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
}
