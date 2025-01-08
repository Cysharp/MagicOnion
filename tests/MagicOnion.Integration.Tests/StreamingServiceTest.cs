using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Client;
using MagicOnion.Client.DynamicClient;
using MagicOnion.Server;

namespace MagicOnion.Integration.Tests;

public class StreamingServiceTest : IClassFixture<MagicOnionApplicationFactory<StreamingTestService>>
{
    readonly MagicOnionApplicationFactory<StreamingTestService> factory;

    public StreamingServiceTest(MagicOnionApplicationFactory<StreamingTestService> factory)
    {
        this.factory = factory;
    }

    public static IEnumerable<object[]> EnumerateMagicOnionClientFactory()
    {
        yield return new [] { new TestMagicOnionClientFactory("Dynamic", DynamicMagicOnionClientFactoryProvider.Instance) };
        yield return new [] { new TestMagicOnionClientFactory("Generated", MagicOnionGeneratedClientInitializer.ClientFactoryProvider) };
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ClientStreaming_1(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IStreamingTestService>(channel);
        var result  = await client.ClientStreaming();

        // Act
        await result.RequestStream.WriteAsync((123, 456), CancellationToken.None);
        await result.RequestStream.WriteAsync((789, 123), CancellationToken.None);
        await result.RequestStream.CompleteAsync();
        var response = await result.ResponseAsync;

        // Assert
        response.Should().Be(123 + 456 + 789 + 123);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ServerStreaming_1(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IStreamingTestService>(channel);
        var result  = await client.ServerStreaming(123, 456);

        // Act
        var sum = 0;
        await foreach (var item in result.ResponseStream.ReadAllAsync(CancellationToken.None))
        {
            sum += item;
        }

        // Assert
        sum.Should().Be(Enumerable.Range(0, 10).Sum(x => (123 + 456) * x));
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task DuplexStreaming_1(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IStreamingTestService>(channel);
        var result  = await client.DuplexStreaming();

        // Act
        var sum = 0;
        var readResponseTask = Task.Run(async () =>
        {
            await foreach (var response in result.ResponseStream.ReadAllAsync(TestContext.Current.CancellationToken))
            {
                sum += response;
            }
        }, TestContext.Current.CancellationToken);
        await result.RequestStream.WriteAsync((123, 456), CancellationToken.None);
        await result.RequestStream.WriteAsync((789, 123), CancellationToken.None);
        await result.RequestStream.WriteAsync((111, 222), CancellationToken.None);
        await result.RequestStream.CompleteAsync();
        await readResponseTask;

        // Assert
        sum.Should().Be(123 + 456 + 789 + 123 + 111 + 222);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ClientStreamingRefType_1(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IStreamingTestService>(channel);
        var result  = await client.ClientStreamingRefType();

        // Act
        await result.RequestStream.WriteAsync(new MyStreamingRequest(123, 456), CancellationToken.None);
        await result.RequestStream.WriteAsync(new MyStreamingRequest(789, 123), CancellationToken.None);
        await result.RequestStream.CompleteAsync();
        var response = await result.ResponseAsync;

        // Assert
        response.Value.Should().Be(123 + 456 + 789 + 123);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ServerStreamingRefType_1(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IStreamingTestService>(channel);
        var result  = await client.ServerStreamingRefType(new MyStreamingRequest(123, 456));

        // Act
        var sum = 0;
        await foreach (var item in result.ResponseStream.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            sum += item.Value;
        }

        // Assert
        sum.Should().Be(Enumerable.Range(0, 10).Sum(x => (123 + 456) * x));
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task DuplexStreamingRefType_1(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IStreamingTestService>(channel);
        var result  = await client.DuplexStreamingRefType();

        // Act
        var sum = 0;
        var readResponseTask = Task.Run(async () =>
        {
            await foreach (var response in result.ResponseStream.ReadAllAsync(TestContext.Current.CancellationToken))
            {
                sum += response.Value;
            }
        }, TestContext.Current.CancellationToken);
        await result.RequestStream.WriteAsync(new MyStreamingRequest(123, 456), CancellationToken.None);
        await result.RequestStream.WriteAsync(new MyStreamingRequest(789, 123), CancellationToken.None);
        await result.RequestStream.WriteAsync(new MyStreamingRequest(111, 222), CancellationToken.None);
        await result.RequestStream.CompleteAsync();
        await readResponseTask;

        // Assert
        sum.Should().Be(123 + 456 + 789 + 123 + 111 + 222);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ClientStreamingRefType_RequestResponseNull(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IStreamingTestService>(channel);
        var result  = await client.ClientStreamingRefTypeReturnsNull();

        // Act
        await result.RequestStream.WriteAsync(null, CancellationToken.None);
        await result.RequestStream.WriteAsync(null, CancellationToken.None);
        await result.RequestStream.CompleteAsync();
        var response = await result.ResponseAsync;

        // Assert
        response.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ServerStreamingRefType_RequestResponseNull(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IStreamingTestService>(channel);
        var result  = await client.ServerStreamingRefTypeReturnsNull(null);

        // Act
        var nullResponseCount = 0;
        await foreach (var response in result.ResponseStream.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            nullResponseCount += response is null ? 1 : 0;
        }

        // Assert
        nullResponseCount.Should().Be(10);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task DuplexStreamingRefType_RequestResponseNull(TestMagicOnionClientFactory clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create<IStreamingTestService>(channel);
        var result  = await client.DuplexStreamingRefTypeReturnsNull();

        // Act
        var nullResponseCount = 0;
        var readResponseTask = Task.Run(async () =>
        {
            await foreach (var response in result.ResponseStream.ReadAllAsync(TestContext.Current.CancellationToken))
            {
                nullResponseCount += response is null ? 1 : 0;
            }
        }, TestContext.Current.CancellationToken);
        await result.RequestStream.WriteAsync(null, CancellationToken.None);
        await result.RequestStream.WriteAsync(null, CancellationToken.None);
        await result.RequestStream.WriteAsync(null, CancellationToken.None);
        await result.RequestStream.CompleteAsync();
        await readResponseTask;

        // Assert
        nullResponseCount.Should().Be(3);
    }
}

public interface IStreamingTestService : IService<IStreamingTestService>
{
    Task<ClientStreamingResult<(int Argument0, int Argument1), int>> ClientStreaming();
    Task<ServerStreamingResult<int>> ServerStreaming(int arg0, int arg1);
    Task<DuplexStreamingResult<(int Argument0, int Argument1), int>> DuplexStreaming();

    Task<ClientStreamingResult<MyStreamingRequest, MyStreamingResponse>> ClientStreamingRefType();
    Task<ServerStreamingResult<MyStreamingResponse>> ServerStreamingRefType(MyStreamingRequest request);
    Task<DuplexStreamingResult<MyStreamingRequest, MyStreamingResponse>> DuplexStreamingRefType();

    Task<ClientStreamingResult<MyStreamingRequest?, MyStreamingResponse?>> ClientStreamingRefTypeReturnsNull();
    Task<ServerStreamingResult<MyStreamingResponse?>> ServerStreamingRefTypeReturnsNull(MyStreamingRequest? request);
    Task<DuplexStreamingResult<MyStreamingRequest?, MyStreamingResponse?>> DuplexStreamingRefTypeReturnsNull();
}

[MessagePackObject(true)]
public record MyStreamingRequest(int Argument0, int Argument1);
[MessagePackObject(true)]
public record MyStreamingResponse(int Value);

public class StreamingTestService : ServiceBase<IStreamingTestService>, IStreamingTestService
{
    public async Task<ClientStreamingResult<(int Argument0, int Argument1), int>> ClientStreaming()
    {
        await Task.Yield();

        var context = GetClientStreamingContext<(int Argument0, int Argument1), int>();

        var sum = 0;
        await foreach (var value in context.ReadAllAsync())
        {
            sum += (value.Argument0 + value.Argument1);
        }

        return context.Result(sum);
    }

    public async Task<ServerStreamingResult<int>> ServerStreaming(int arg0, int arg1)
    {
        await Task.Yield();

        var context = GetServerStreamingContext<int>();

        for (var i = 0; i < 10; i++)
        {
            await context.WriteAsync((arg0 + arg1) * i);
        }

        return context.Result();
    }

    public async Task<DuplexStreamingResult<(int Argument0, int Argument1), int>> DuplexStreaming()
    {
        await Task.Yield();

        var context = GetDuplexStreamingContext<(int Argument0, int Argument1), int>();

        await foreach (var value in context.ReadAllAsync())
        {
            await context.WriteAsync(value.Argument0 + value.Argument1);
        }

        return context.Result();
    }

    public async Task<ClientStreamingResult<MyStreamingRequest, MyStreamingResponse>> ClientStreamingRefType()
    {
        await Task.Yield();

        var context = GetClientStreamingContext<MyStreamingRequest, MyStreamingResponse>();

        var sum = 0;
        await foreach (var value in context.ReadAllAsync())
        {
            sum += (value.Argument0 + value.Argument1);
        }

        return context.Result(new MyStreamingResponse(sum));

    }

    public async Task<ServerStreamingResult<MyStreamingResponse>> ServerStreamingRefType(MyStreamingRequest request)
    {
        await Task.Yield();

        var context = GetServerStreamingContext<MyStreamingResponse>();

        for (var i = 0; i < 10; i++)
        {
            await context.WriteAsync(new MyStreamingResponse((request.Argument0 + request.Argument1) * i));
        }

        return context.Result();
    }

    public async Task<DuplexStreamingResult<MyStreamingRequest, MyStreamingResponse>> DuplexStreamingRefType()
    {
        await Task.Yield();

        var context = GetDuplexStreamingContext<MyStreamingRequest, MyStreamingResponse>();

        await foreach (var value in context.ReadAllAsync())
        {
            await context.WriteAsync(new MyStreamingResponse(value.Argument0 + value.Argument1));
        }

        return context.Result();
    }


    public async Task<ClientStreamingResult<MyStreamingRequest?, MyStreamingResponse?>> ClientStreamingRefTypeReturnsNull()
    {
        await Task.Yield();

        var context = GetClientStreamingContext<MyStreamingRequest?, MyStreamingResponse?>();

        var sum = 0;
        await foreach (var value in context.ReadAllAsync())
        {
            if (value is null) continue;
            sum += (value.Argument0 + value.Argument1);
        }

        return context.Result(null);

    }

    public async Task<ServerStreamingResult<MyStreamingResponse?>> ServerStreamingRefTypeReturnsNull(MyStreamingRequest? request)
    {
        await Task.Yield();

        var context = GetServerStreamingContext<MyStreamingResponse?>();

        for (var i = 0; i < 10; i++)
        {
            await context.WriteAsync(null);
        }

        return context.Result();
    }

    public async Task<DuplexStreamingResult<MyStreamingRequest?, MyStreamingResponse?>> DuplexStreamingRefTypeReturnsNull()
    {
        await Task.Yield();

        var context = GetDuplexStreamingContext<MyStreamingRequest?, MyStreamingResponse?>();

        await foreach (var value in context.ReadAllAsync())
        {
            Debug.Assert(value is null);
            await context.WriteAsync(null);
        }

        return context.Result();
    }
}
