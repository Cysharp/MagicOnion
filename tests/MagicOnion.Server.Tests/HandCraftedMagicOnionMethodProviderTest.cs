using System.Runtime.CompilerServices;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Internal;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Tests;

public class HandCraftedMagicOnionMethodProviderTest(HandCraftedMagicOnionMethodProviderTest.ApplicationFactory factory) : IClassFixture<HandCraftedMagicOnionMethodProviderTest.ApplicationFactory>
{
    public class ApplicationFactory : MagicOnionApplicationFactory
    {
        protected override IEnumerable<Type> GetServiceImplementationTypes()
        {
            yield return typeof(HandCraftedMagicOnionMethodProviderTest_GreeterService);
            yield return typeof(HandCraftedMagicOnionMethodProviderTest_GreeterService2);
            yield return typeof(HandCraftedMagicOnionMethodProviderTest_GreeterHub);
        }
    }

    [Fact]
    public async Task Services()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var client1 = MagicOnionClient.Create<IHandCraftedMagicOnionMethodProviderTest_GreeterService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));
        var client2 = MagicOnionClient.Create<IHandCraftedMagicOnionMethodProviderTest_GreeterService2>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        // Act
        var result1 = await client1.HelloAsync("Alice", 18);
        var result2 = await client2.GoodByeAsync("Alice", 18);

        // Assert
        Assert.Equal("Hello Alice (18) !", result1);
        Assert.Equal("Goodbye Alice (18) !", result2);
    }

    [Fact]
    public async Task Unary_Parameter_Many_Return_RefType()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IHandCraftedMagicOnionMethodProviderTest_GreeterService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        // Act
        var result = await client.HelloAsync("Alice", 18);

        // Assert
        Assert.Equal("Hello Alice (18) !", result);
    }

    [Fact]
    public async Task Unary_Parameter_Zero_NoReturnValue()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var client = MagicOnionClient.Create<IHandCraftedMagicOnionMethodProviderTest_GreeterService>(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient }));

        // Act & Assert
        await client.PingAsync();
    }
}


public interface IHandCraftedMagicOnionMethodProviderTest_GreeterService : IService<IHandCraftedMagicOnionMethodProviderTest_GreeterService>
{
    UnaryResult<string> HelloAsync(string name, int age);
    UnaryResult PingAsync();
}

public interface IHandCraftedMagicOnionMethodProviderTest_GreeterService2 : IService<IHandCraftedMagicOnionMethodProviderTest_GreeterService2>
{
    UnaryResult<string> GoodByeAsync(string name, int age);
    UnaryResult PingAsync();
}

class HandCraftedMagicOnionMethodProviderTest_GreeterService : ServiceBase<IHandCraftedMagicOnionMethodProviderTest_GreeterService>, IHandCraftedMagicOnionMethodProviderTest_GreeterService
{
    [HandCraftedMagicOnionMethodProviderTest_MyFilter]
    public UnaryResult<string> HelloAsync(string name, int age) => UnaryResult.FromResult($"Hello {name} ({age}) !");
    public UnaryResult PingAsync() => default;
}

class HandCraftedMagicOnionMethodProviderTest_GreeterService2 : ServiceBase<IHandCraftedMagicOnionMethodProviderTest_GreeterService2>, IHandCraftedMagicOnionMethodProviderTest_GreeterService2
{
    [HandCraftedMagicOnionMethodProviderTest_MyFilter]
    public UnaryResult<string> GoodByeAsync(string name, int age) => UnaryResult.FromResult($"Goodbye {name} ({age}) !");
    public UnaryResult PingAsync() => default;
}

public interface IHandCraftedMagicOnionMethodProviderTest_GreeterHub : IStreamingHub<IHandCraftedMagicOnionMethodProviderTest_GreeterHub, IHandCraftedMagicOnionMethodProviderTest_GreeterHubReceiver>
{
    Task JoinAsync(string name, string channel);
    ValueTask SendMessageAsync(string message);
    ValueTask<IReadOnlyList<string>> GetMembersAsync();
}

public interface IHandCraftedMagicOnionMethodProviderTest_GreeterHubReceiver
{
    void OnMessage(string message);
}

class HandCraftedMagicOnionMethodProviderTest_GreeterHub : StreamingHubBase<IHandCraftedMagicOnionMethodProviderTest_GreeterHub, IHandCraftedMagicOnionMethodProviderTest_GreeterHubReceiver>, IHandCraftedMagicOnionMethodProviderTest_GreeterHub
{
    public Task JoinAsync(string name, string channel)
    {
        throw new NotImplementedException();
    }

    public ValueTask SendMessageAsync(string message)
    {
        throw new NotImplementedException();
    }

    public ValueTask<IReadOnlyList<string>> GetMembersAsync()
    {
        throw new NotImplementedException();
    }
}

class HandCraftedMagicOnionMethodProviderTest_MyFilter : MagicOnionFilterAttribute
{
    public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        return next(context);
    }
}

internal class HandCraftedMagicOnionMethodProviderTest_GeneratedMagicOnionMethodProvider : IMagicOnionGrpcMethodProvider
{
    public void MapAllSupportedServiceTypes(MagicOnionGrpcServiceMappingContext context)
    {
        context.Map<HandCraftedMagicOnionMethodProviderTest_GreeterService>();
        context.Map<HandCraftedMagicOnionMethodProviderTest_GreeterService2>();
        context.Map<HandCraftedMagicOnionMethodProviderTest_GreeterHub>();
    }

    public IEnumerable<IMagicOnionGrpcMethod> GetGrpcMethods<TService>() where TService : class
    {
        if (typeof(TService) == typeof(HandCraftedMagicOnionMethodProviderTest_GreeterService))
        {
            yield return new MagicOnionUnaryMethod<HandCraftedMagicOnionMethodProviderTest_GreeterService, DynamicArgumentTuple<string, int>, string, Box<DynamicArgumentTuple<string, int>>, string>(
                nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterService), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterService.HelloAsync), static (instance, context, request) => instance.HelloAsync(request.Item1, request.Item2));
            yield return new MagicOnionUnaryMethod<HandCraftedMagicOnionMethodProviderTest_GreeterService, MessagePack.Nil, Box<MessagePack.Nil>>(
                nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterService), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterService.PingAsync), static (instance, context, request) => instance.PingAsync());
        }
        if (typeof(TService) == typeof(HandCraftedMagicOnionMethodProviderTest_GreeterService2))
        {
            yield return new MagicOnionUnaryMethod<HandCraftedMagicOnionMethodProviderTest_GreeterService2, DynamicArgumentTuple<string, int>, string, Box<DynamicArgumentTuple<string, int>>, string>(
                nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterService2), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterService2.GoodByeAsync), static (instance, context, request) => instance.GoodByeAsync(request.Item1, request.Item2));
            yield return new MagicOnionUnaryMethod<HandCraftedMagicOnionMethodProviderTest_GreeterService2, MessagePack.Nil, Box<MessagePack.Nil>>(
                nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterService2), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterService2.PingAsync), static (instance, context, request) => instance.PingAsync());
        }

        if (typeof(TService) == typeof(HandCraftedMagicOnionMethodProviderTest_GreeterHub))
        {
            yield return new MagicOnionStreamingHubConnectMethod<HandCraftedMagicOnionMethodProviderTest_GreeterHub>(nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub));
        }
    }

    public IEnumerable<IMagicOnionStreamingHubMethod> GetStreamingHubMethods<TService>() where TService : class
    {
        if (typeof(TService) == typeof(HandCraftedMagicOnionMethodProviderTest_GreeterHub))
        {
            yield return new MagicOnionStreamingHubMethod<HandCraftedMagicOnionMethodProviderTest_GreeterHub, DynamicArgumentTuple<string, string>>(
                nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub.JoinAsync), static (instance, context, request) => instance.JoinAsync(request.Item1, request.Item2));
            yield return new MagicOnionStreamingHubMethod<HandCraftedMagicOnionMethodProviderTest_GreeterHub, string>(
                nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub.SendMessageAsync), static (instance, context, request) => instance.SendMessageAsync(request));
            yield return new MagicOnionStreamingHubMethod<HandCraftedMagicOnionMethodProviderTest_GreeterHub, MessagePack.Nil, IReadOnlyList<string>>(
                nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub.GetMembersAsync), static (instance, context, request) => instance.GetMembersAsync());
            //yield return new MagicOnionStreamingHubMethod<HandCraftedMagicOnionMethodProviderTest_GreeterHub, DynamicArgumentTuple<string, string>>(
            //    nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub.JoinAsync), typeof(HandCraftedMagicOnionMethodProviderTest_GreeterHub).GetMethod(nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub.JoinAsync))!);
            //yield return new MagicOnionStreamingHubMethod<HandCraftedMagicOnionMethodProviderTest_GreeterHub, string>(
            //    nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub.SendMessageAsync), typeof(HandCraftedMagicOnionMethodProviderTest_GreeterHub).GetMethod(nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub.SendMessageAsync))!);
            //yield return new MagicOnionStreamingHubMethod<HandCraftedMagicOnionMethodProviderTest_GreeterHub, MessagePack.Nil, IReadOnlyList<string>>(
            //    nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub), nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub.GetMembersAsync), typeof(HandCraftedMagicOnionMethodProviderTest_GreeterHub).GetMethod(nameof(IHandCraftedMagicOnionMethodProviderTest_GreeterHub.GetMembersAsync))!);
        }
    }
}
