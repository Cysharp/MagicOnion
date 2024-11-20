using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.JsonTranscoding.Tests;

public class UnaryFunctionalTests(JsonTranscodingEnabledMagicOnionApplicationFactory<TestService> factory) : IClassFixture<JsonTranscodingEnabledMagicOnionApplicationFactory<TestService>>
{
    [Fact]
    public async Task NotImplemented()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/_/ITestService/NotImplemented", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Method_NoParameter_NoResult()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/_/ITestService/Method_NoParameter_NoResult", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var result = default(object); // null
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(result, JsonSerializer.Deserialize<object>(content));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
        Assert.True((bool)factory.Items.GetValueOrDefault($"{nameof(Method_NoParameter_NoResult)}.Called", false));
    }

    [Fact]
    public async Task Method_NoParameter_ResultRefType()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/_/ITestService/Method_NoParameter_ResultRefType", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var result = nameof(Method_NoParameter_ResultRefType); // string
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(result, JsonSerializer.Deserialize<string>(content));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Method_NoParameter_ResultComplexType()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/_/ITestService/Method_NoParameter_ResultComplexType", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        object[] result = [1234, "Alice", true, new object[] { 98765432100, "Hello!" }];
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(content)));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }
}

public interface ITestService : IService<ITestService>
{
    UnaryResult Method_NoParameter_NoResult();
    UnaryResult<string> Method_NoParameter_ResultRefType();
    UnaryResult<TestResponse> Method_NoParameter_ResultComplexType();
}

[MessagePackObject]
public class TestResponse
{
    [Key(0)]
    public int A { get; set; }
    [Key(1)]
    public required string B { get; init; }
    [Key(2)]
    public bool C { get; set; }
    [Key(3)]
    public required InnerResponse Inner { get; init; }

    [MessagePackObject]
    public class InnerResponse
    {
        [Key(0)]
        public long D { get; set; }
        [Key(1)]
        public required string E { get; init; }
    }
}

public class TestService([FromKeyedServices(MagicOnionApplicationFactory.ItemsKey)] ConcurrentDictionary<string, object> items) : ServiceBase<ITestService>, ITestService
{
    public UnaryResult Method_NoParameter_NoResult()
    {
        items[$"{nameof(Method_NoParameter_NoResult)}.Called"] = true;
        return default;
    }

public UnaryResult<string> Method_NoParameter_ResultRefType()
        => UnaryResult.FromResult(nameof(Method_NoParameter_ResultRefType));

    public UnaryResult<TestResponse> Method_NoParameter_ResultComplexType()
        => UnaryResult.FromResult(new TestResponse()
        {
            A = 1234,
            B = "Alice",
            C = true,
            Inner = new TestResponse.InnerResponse()
            {
                D = 98765432100,
                E = "Hello!",
            },
        });
}
