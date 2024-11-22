using System.Net;
using System.Net.Http.Headers;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.JsonTranscoding.Tests;

public class NonUnaryMethodsTest(NonUnaryMethodsTest.ApplicationFactory factory) : IClassFixture<NonUnaryMethodsTest.ApplicationFactory>
{
    public class ApplicationFactory : JsonTranscodingEnabledMagicOnionApplicationFactory
    {
        protected override IEnumerable<Type> GetServiceImplementationTypes() => [typeof(TestHub), typeof(NotSupportedMethodsService)];
    }

    [Fact]
    public async Task IgnoreStreamingHub()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestHub/Connect", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task NotImplemented_ServerStreaming()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/INotSupportedMethodsService/ServerStreamingMethod", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task NotImplemented_ClientStreaming()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/INotSupportedMethodsService/ClientStreamingMethod", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task NotImplemented_DuplexStreaming()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/INotSupportedMethodsService/DuplexStreamingMethod", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}

public class TestHub : StreamingHubBase<ITestHub, ITestHubReceiver>, ITestHub;

public interface ITestHub : IStreamingHub<ITestHub, ITestHubReceiver>;
public interface ITestHubReceiver;


public interface INotSupportedMethodsService : IService<INotSupportedMethodsService>
{
    Task<ServerStreamingResult<int>> ServerStreamingMethod();
    Task<ClientStreamingResult<int,int>> ClientStreamingMethod();
    Task<DuplexStreamingResult<int, int>> DuplexStreamingMethod();
}

public class NotSupportedMethodsService : ServiceBase<INotSupportedMethodsService>, INotSupportedMethodsService
{
    public Task<ServerStreamingResult<int>> ServerStreamingMethod() => throw new NotImplementedException();
    public Task<ClientStreamingResult<int, int>> ClientStreamingMethod() => throw new NotImplementedException();
    public Task<DuplexStreamingResult<int, int>> DuplexStreamingMethod() => throw new NotImplementedException();
}
