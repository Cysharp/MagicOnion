using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Grpc.Core;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Sdk;

namespace MagicOnion.Server.JsonTranscoding.Tests;

public class UnaryFunctionalTests(JsonTranscodingEnabledMagicOnionApplicationFactory<TestService> factory) : IClassFixture<JsonTranscodingEnabledMagicOnionApplicationFactory<TestService>>
{
    [Fact]
    public async Task NotImplemented()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/NotImplemented", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Method_NoParameter_NoResult()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/Method_NoParameter_NoResult", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var result = default(object); // null
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(result, JsonSerializer.Deserialize<object>(content));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
        Assert.True((bool)factory.Items.GetValueOrDefault($"{nameof(Method_NoParameter_NoResult)}.Called", false));
    }

    [Fact]
    public async Task Method_NoParameter_NoResult_IgnoreBody()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var requestBody = "{}";

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/Method_NoParameter_NoResult", new StringContent(requestBody, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var result = default(object); // null
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(result, JsonSerializer.Deserialize<object>(content, JsonSerializerOptions.Web));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
        Assert.True((bool)factory.Items.GetValueOrDefault($"{nameof(Method_NoParameter_NoResult)}.Called", false));
    }

    [Fact]
    public async Task Method_NoParameter_ResultRefType()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/Method_NoParameter_ResultRefType", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var result = nameof(Method_NoParameter_ResultRefType); // string
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(result, JsonSerializer.Deserialize<string>(content, JsonSerializerOptions.Web));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Method_NoParameter_ResultComplexType()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/Method_NoParameter_ResultComplexType", new StringContent(string.Empty, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equivalent(new TestResponse()
        {
            A = 1234,
            B = "Alice",
            C = true,
            Inner = new TestResponse.InnerResponse()
            {
                D = 98765432100,
                E = "Hello!",
            },
        }, JsonSerializer.Deserialize<TestResponse>(content, JsonSerializerOptions.Web));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Method_OneParameter_NoResult()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var requestBody = """
                          "Alice"
                          """;

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/Method_OneParameter_NoResult", new StringContent(requestBody, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Alice", JsonSerializer.Deserialize<string>(content, JsonSerializerOptions.Web));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Method_TwoParameter_NoResult()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var requestBody = """
                          ["Alice", 18]
                          """;

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/Method_TwoParameter_NoResult", new StringContent(requestBody, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Alice;18", JsonSerializer.Deserialize<string>(content));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Method_ManyParameter_NoResult()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var requestBody = """
                          ["Alice", 18, true, 128, 3.14, null]
                          """;

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/Method_ManyParameter_NoResult", new StringContent(requestBody, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Alice;18;True;128;3.14;null", JsonSerializer.Deserialize<string>(content));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Method_TwoParameter_NoResult_Keyed()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var requestBody = """
                          { "name": "Alice", "age": 18 }
                          """;

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/Method_TwoParameter_NoResult", new StringContent(requestBody, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Alice;18", JsonSerializer.Deserialize<string>(content));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Method_ManyParameter_NoResult_Keyed()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var requestBody = """
                          { "arg1": "Alice", "arg2": 18, "arg3": true, "arg4": 128, "arg5": 3.14, "arg6": null }
                          """;

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/Method_ManyParameter_NoResult", new StringContent(requestBody, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Alice;18;True;128;3.14;null", JsonSerializer.Deserialize<string>(content));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Throw()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var requestBody = """
                          {}
                          """;

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/ThrowAsync", new StringContent(requestBody, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task ThrowWithReturnStatusCode()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        // StatusCode.Unavailable = 14
        var requestBody = """
                          { "statusCode": 14, "detail": "DetailMessage" }
                          """;

        // Act
        var response = await httpClient.PostAsync($"http://localhost/webapi/ITestService/ThrowWithReturnStatusCodeAsync", new StringContent(requestBody, new MediaTypeHeaderValue("application/json")));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(new ErrorResponse(14, "DetailMessage"), JsonSerializer.Deserialize<ErrorResponse>(content));
        Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
    }

    record ErrorResponse(int Code, string Detail);
}

public interface ITestService : IService<ITestService>
{
    UnaryResult Method_NoParameter_NoResult();
    UnaryResult<string> Method_NoParameter_ResultRefType();
    UnaryResult<TestResponse> Method_NoParameter_ResultComplexType();

    UnaryResult<string> Method_OneParameter_NoResult(string name);
    UnaryResult<string> Method_TwoParameter_NoResult(string name, int age);
    UnaryResult<string> Method_ManyParameter_NoResult(string arg1, int arg2, bool arg3, byte arg4, float arg5, string arg6);

    UnaryResult ThrowAsync();
    UnaryResult ThrowWithReturnStatusCodeAsync(int statusCode, string detail);
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


    public UnaryResult<string> Method_OneParameter_NoResult(string name)
        => UnaryResult.FromResult($"{name}");
    public UnaryResult<string> Method_TwoParameter_NoResult(string name, int age)
        => UnaryResult.FromResult($"{name};{age}");
    public UnaryResult<string> Method_ManyParameter_NoResult(string arg1, int arg2, bool arg3, byte arg4, float arg5, string arg6)
        => UnaryResult.FromResult($"{arg1};{arg2};{arg3};{arg4};{arg5};{arg6 ?? "null"}");

    public UnaryResult ThrowAsync()
    {
        throw new InvalidOperationException("Something went wrong.");
    }

    public UnaryResult ThrowWithReturnStatusCodeAsync(int statusCode, string detail)
    {
        throw new ReturnStatusException((StatusCode)statusCode, detail);
    }
}
