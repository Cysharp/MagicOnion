using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Client;
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
        yield return new [] { new TestMagicOnionClientFactory<IStreamingTestService>("Dynamic", x => MagicOnionClient.Create<IStreamingTestService>(x, MagicOnionMessageSerializer.Default)) };
        yield return new [] { new TestMagicOnionClientFactory<IStreamingTestService>("Generated", x => new StreamingTestServiceClient(x, MagicOnionMessageSerializer.Default)) };
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ClientStreaming_1(TestMagicOnionClientFactory<IStreamingTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel);
        var result  = await client.ClientStreaming();

        // Act
        await result.RequestStream.WriteAsync((123, 456));
        await result.RequestStream.WriteAsync((789, 123));
        await result.RequestStream.CompleteAsync();
        var response = await result.ResponseAsync;

        // Assert
        response.Should().Be(123 + 456 + 789 + 123);
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ServerStreaming_1(TestMagicOnionClientFactory<IStreamingTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel);
        var result  = await client.ServerStreaming(123, 456);

        // Act
        var sum = 0;
        await foreach (var item in result.ResponseStream.ReadAllAsync())
        {
            sum += item;
        }

        // Assert
        sum.Should().Be(Enumerable.Range(0, 10).Sum(x => (123 + 456) * x));
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task DuplexStreaming_1(TestMagicOnionClientFactory<IStreamingTestService> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var client = clientFactory.Create(channel);
        var result  = await client.DuplexStreaming();

        // Act
        var sum = 0;
        var readResponseTask = Task.Run(async () =>
        {
            await foreach (var response in result.ResponseStream.ReadAllAsync())
            {
                sum += response;
            }
        });
        await result.RequestStream.WriteAsync((123, 456));
        await result.RequestStream.WriteAsync((789, 123));
        await result.RequestStream.WriteAsync((111, 222));
        await result.RequestStream.CompleteAsync();
        await readResponseTask;

        // Assert
        sum.Should().Be(123 + 456 + 789 + 123 + 111 + 222);
    }
}

public interface IStreamingTestService : IService<IStreamingTestService>
{
    Task<ClientStreamingResult<(int Argument0, int Argument1), int>> ClientStreaming();
    Task<ServerStreamingResult<int>> ServerStreaming(int arg0, int arg1);
    Task<DuplexStreamingResult<(int Argument0, int Argument1), int>> DuplexStreaming();
}

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
}
