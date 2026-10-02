using System.Reflection;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.Internal;
using MessagePack;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace MagicOnion.Server.NativeAot.Tests;

/// <summary>Exercises the metadata path used by generated providers in both JIT and Native AOT.</summary>
public class MethodMetadataTest
{
    // Wire names deliberately differ from implementation names to detect accidental method discovery.
    const string WireMethodName = "GeneratedMethod";

    /// <summary>Preserves the empty request and response types of a parameterless unary method.</summary>
    [Fact]
    public void ParameterlessUnary()
    {
        var metadata = CreateServiceMetadata(typeof(MetadataService).GetMethod(nameof(MetadataService.Ping))!, MethodType.Unary, typeof(Nil), typeof(Nil));
        var method = new MagicOnionUnaryMethod<MetadataService, Nil, Box<Nil>>(
            "Service", WireMethodName, metadata, static (service, _, _) => service.Ping());

        Assert.Same(metadata, method.Metadata);
        Assert.Equal(WireMethodName, method.MethodName);
        Assert.Empty(metadata.Parameters);
        Assert.Equal(typeof(Nil), metadata.RequestType);
        Assert.Equal(typeof(Nil), metadata.ResponseType);
    }

    /// <summary>Uses the single argument's type without constructing an argument tuple.</summary>
    [Fact]
    public void SingleParameterUnary()
    {
        var metadata = CreateServiceMetadata(typeof(MetadataService).GetMethod(nameof(MetadataService.Echo))!, MethodType.Unary, typeof(int), typeof(int));
        var method = new MagicOnionUnaryMethod<MetadataService, int, int, Box<int>, Box<int>>(
            "Service", WireMethodName, metadata, static (service, _, request) => service.Echo(request));

        Assert.Same(metadata, method.Metadata);
        Assert.Equal(typeof(int), metadata.RequestType);
        Assert.Equal(typeof(int), metadata.ResponseType);
        Assert.Equal("value", Assert.Single(metadata.Parameters).Name);
    }

    /// <summary>Preserves implementation defaults and inherited attributes without rediscovering message types.</summary>
    [Fact]
    public void MultipleParametersAndInheritedAttributes()
    {
        var implementation = typeof(MetadataService).GetMethod(nameof(MetadataService.Greet), [typeof(string), typeof(int)])!;
        var metadata = CreateServiceMetadata(implementation, MethodType.Unary, typeof(DynamicArgumentTuple<string, int>), typeof(string));
        var method = new MagicOnionUnaryMethod<MetadataService, DynamicArgumentTuple<string, int>, string, Box<DynamicArgumentTuple<string, int>>, string>(
            "Service", WireMethodName, metadata, static (service, _, request) => service.Greet(request.Item1, request.Item2));

        Assert.Same(metadata, method.Metadata);
        Assert.Equal(typeof(MetadataService), metadata.ServiceImplementationType);
        Assert.Equal(typeof(IMetadataService), metadata.ServiceInterface);
        Assert.Equal(implementation, metadata.ServiceImplementationMethod);
        Assert.Equal(typeof(DynamicArgumentTuple<string, int>), metadata.RequestType);
        Assert.Equal(typeof(string), metadata.ResponseType);
        Assert.Equal(new[] { "name", "age" }, metadata.Parameters.Select(x => x.Name));
        Assert.Equal(18, metadata.Parameters[1].DefaultValue);
        Assert.Equal(new[] { "service", "base-service", "method", "base-method-1", "base-method-2" },
            metadata.AttributeLookup[typeof(MetadataMarkerAttribute)].Cast<MetadataMarkerAttribute>().Select(x => x.Value));
        var httpMetadata = Assert.IsType<HttpMethodMetadata>(metadata.Metadata.Last());
        Assert.Equal(new[] { "POST" }, httpMetadata.HttpMethods);
        Assert.True(httpMetadata.AcceptCorsPreflight);
    }

    /// <summary>Keeps non-public explicit implementation metadata separate from the contract method.</summary>
    [Fact]
    public void ExplicitImplementation()
    {
        var implementation = typeof(ExplicitMetadataService).GetMethod(
            "MagicOnion.Server.NativeAot.Tests.IExplicitMetadataService.Echo", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var metadata = MethodHandlerMetadata.Create(typeof(ExplicitMetadataService), implementation, MethodType.Unary,
            typeof(string), typeof(string), typeof(IExplicitMetadataService));
        var method = new MagicOnionUnaryMethod<ExplicitMetadataService, string, string, string, string>(
            "Service", WireMethodName, metadata, static (service, _, request) => ((IExplicitMetadataService)service).Echo(request));

        Assert.Same(metadata, method.Metadata);
        Assert.Equal(implementation, metadata.ServiceImplementationMethod);
        Assert.Equal("explicit", Assert.IsType<MetadataMarkerAttribute>(Assert.Single(metadata.AttributeLookup[typeof(MetadataMarkerAttribute)])).Value);
    }

    /// <summary>Initializes all streaming method kinds through their supplied metadata and invokes their delegates.</summary>
    [Fact]
    public async Task StreamingMethods()
    {
        var clientMetadata = CreateServiceMetadata(typeof(MetadataService).GetMethod(nameof(MetadataService.Upload))!, MethodType.ClientStreaming, typeof(int), typeof(string));
        var serverMetadata = CreateServiceMetadata(typeof(MetadataService).GetMethod(nameof(MetadataService.Download))!, MethodType.ServerStreaming, typeof(DynamicArgumentTuple<int, string>), typeof(string));
        var duplexMetadata = CreateServiceMetadata(typeof(MetadataService).GetMethod(nameof(MetadataService.Duplex))!, MethodType.DuplexStreaming, typeof(int), typeof(string));
        var client = new MagicOnionClientStreamingMethod<MetadataService, int, string, Box<int>, string>(
            "Service", WireMethodName, clientMetadata, static (service, _) => service.Upload());
        var server = new MagicOnionServerStreamingMethod<MetadataService, DynamicArgumentTuple<int, string>, string, Box<DynamicArgumentTuple<int, string>>, string>(
            "Service", WireMethodName, serverMetadata, static (service, _, request) => service.Download(request.Item1, request.Item2));
        var duplex = new MagicOnionDuplexStreamingMethod<MetadataService, int, string, Box<int>, string>(
            "Service", WireMethodName, duplexMetadata, static (service, _) => service.Duplex());

        Assert.Same(clientMetadata, client.Metadata);
        Assert.Same(serverMetadata, server.Metadata);
        Assert.Same(duplexMetadata, duplex.Metadata);
        Assert.Equal(MethodType.ClientStreaming, client.Metadata.MethodType);
        Assert.Equal(MethodType.ServerStreaming, server.Metadata.MethodType);
        Assert.Equal(MethodType.DuplexStreaming, duplex.Metadata.MethodType);
        Assert.Empty(client.Metadata.Parameters);
        Assert.Equal(2, server.Metadata.Parameters.Count);
        Assert.Empty(duplex.Metadata.Parameters);

        var service = new MetadataService();
        // These delegates return empty streaming results and do not access the service context.
        await client.InvokeAsync(service, null!);
        await server.InvokeAsync(service, null!, new(42, "payload"));
        await duplex.InvokeAsync(service, null!);
        Assert.Equal(new[] { "upload", "42:payload", "duplex" }, service.Calls);
    }

    /// <summary>Preserves a hub contract method declared on a parent interface and its explicit identifier.</summary>
    [Fact]
    public void InheritedHubContract()
    {
        var contract = typeof(IMetadataHubParent).GetMethod(nameof(IMetadataHubParent.Query))!;
        var implementation = typeof(MetadataHub).GetMethod(nameof(MetadataHub.Query))!;
        var metadata = StreamingHubMethodHandlerMetadata.Create(123, typeof(MetadataHub), contract, implementation,
            typeof(int), typeof(DynamicArgumentTuple<string, int>), typeof(IMetadataHub));
        var taskMethod = new MagicOnionStreamingHubMethod<MetadataHub, DynamicArgumentTuple<string, int>, int>(
            "Hub", WireMethodName, metadata, static (hub, _, request) => hub.Query(request.Item1, request.Item2));
        var valueTaskMethod = new MagicOnionStreamingHubMethod<MetadataHub, DynamicArgumentTuple<string, int>, int>(
            "Hub", WireMethodName, metadata, static (hub, _, request) => new ValueTask<int>(hub.Query(request.Item1, request.Item2)));

        Assert.Same(metadata, taskMethod.Metadata);
        Assert.Same(metadata, valueTaskMethod.Metadata);
        Assert.Equal(123, metadata.MethodId);
        Assert.Equal(123, contract.GetCustomAttribute<MethodIdAttribute>()!.MethodId);
        Assert.Equal(typeof(IMetadataHub), metadata.StreamingHubInterfaceType);
        Assert.Equal(typeof(IMetadataHubParent), metadata.InterfaceMethod.DeclaringType);
        Assert.Equal(implementation, metadata.ImplementationMethod);
        Assert.Equal(typeof(int), metadata.ResponseType);
        Assert.Equal(typeof(DynamicArgumentTuple<string, int>), metadata.RequestType);
        Assert.Equal(9, metadata.Parameters[1].DefaultValue);
        Assert.Equal(new[] { "hub", "query" }, metadata.AttributeLookup[typeof(MetadataMarkerAttribute)].Cast<MetadataMarkerAttribute>().Select(x => x.Value));
        Assert.DoesNotContain(metadata.Metadata, x => x is HttpMethodMetadata);
    }

    /// <summary>Preserves the null response type for Task, ValueTask, and void hub methods.</summary>
    [Fact]
    public void HubWithoutResponse()
    {
        var taskMetadata = CreateHubMetadata(typeof(IMetadataHub).GetMethod(nameof(IMetadataHub.Notify))!, typeof(MetadataHub).GetMethod(nameof(MetadataHub.Notify))!, typeof(Nil));
        var valueTaskMetadata = CreateHubMetadata(typeof(IMetadataHub).GetMethod(nameof(IMetadataHub.Update))!, typeof(MetadataHub).GetMethod(nameof(MetadataHub.Update))!, typeof(int));
        var voidMetadata = CreateHubMetadata(typeof(IMetadataHub).GetMethod(nameof(IMetadataHub.Send))!, typeof(MetadataHub).GetMethod(nameof(MetadataHub.Send))!, typeof(string));
        var taskMethod = new MagicOnionStreamingHubMethod<MetadataHub, Nil>("Hub", WireMethodName, taskMetadata, static (hub, _, _) => hub.Notify());
        var valueTaskMethod = new MagicOnionStreamingHubMethod<MetadataHub, int>("Hub", WireMethodName, valueTaskMetadata, static (hub, _, request) => hub.Update(request));
        var voidMethod = new MagicOnionStreamingHubMethod<MetadataHub, string>("Hub", WireMethodName, voidMetadata, static (hub, _, request) => hub.Send(request));

        Assert.Same(taskMetadata, taskMethod.Metadata);
        Assert.Same(valueTaskMetadata, valueTaskMethod.Metadata);
        Assert.Same(voidMetadata, voidMethod.Metadata);
        Assert.Null(taskMetadata.ResponseType);
        Assert.Null(valueTaskMetadata.ResponseType);
        Assert.Null(voidMetadata.ResponseType);
        Assert.Empty(taskMetadata.Parameters);
        Assert.Single(valueTaskMetadata.Parameters);
        Assert.Single(voidMetadata.Parameters);
    }

    /// <summary>Constructs the built-in connection metadata without exposing internal payload types to generated code.</summary>
    [Fact]
    public void StreamingHubConnect()
    {
        var metadata = MethodHandlerMetadata.CreateStreamingHubConnect<MetadataHub, IMetadataHub, IMetadataReceiver>();
        var method = new MagicOnionStreamingHubConnectMethod<MetadataHub>("Hub", metadata);

        Assert.Same(metadata, method.Metadata);
        Assert.Equal("Connect", method.MethodName);
        Assert.Equal(MethodType.DuplexStreaming, metadata.MethodType);
        Assert.Equal(typeof(MetadataHub), metadata.ServiceImplementationType);
        Assert.Equal(typeof(IMetadataHub), metadata.ServiceInterface);
        Assert.Equal("MagicOnion.Server.Internal.IStreamingHubBase.Connect", metadata.ServiceImplementationMethod.Name);
        Assert.Equal(typeof(StreamingHubBase<IMetadataHub, IMetadataReceiver>), metadata.ServiceImplementationMethod.ReflectedType);
        Assert.Equal("StreamingHubPayload", metadata.RequestType.Name);
        Assert.Equal(metadata.RequestType, metadata.ResponseType);
        Assert.Empty(metadata.Parameters);
        Assert.Equal("hub", Assert.IsType<MetadataMarkerAttribute>(Assert.Single(metadata.AttributeLookup[typeof(MetadataMarkerAttribute)])).Value);
        Assert.IsType<HttpMethodMetadata>(metadata.Metadata.Last());
    }

    static MethodHandlerMetadata CreateServiceMetadata(MethodInfo method, MethodType methodType, Type request, Type response)
        => MethodHandlerMetadata.Create(typeof(MetadataService), method, methodType, response, request, typeof(IMetadataService));

    static StreamingHubMethodHandlerMetadata CreateHubMetadata(MethodInfo contract, MethodInfo implementation, Type request)
        => StreamingHubMethodHandlerMetadata.Create(456, typeof(MetadataHub), contract, implementation, null, request, typeof(IMetadataHub));
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal sealed class MetadataMarkerAttribute(string value) : Attribute
{
    public string Value { get; } = value;
}

[MetadataMarker("contract")]
internal interface IMetadataService : IService<IMetadataService>
{
    UnaryResult Ping();
    UnaryResult<int> Echo(int value);
    [MetadataMarker("contract-method")]
    UnaryResult<string> Greet(string name, int age = 99);
    Task<ClientStreamingResult<int, string>> Upload();
    Task<ServerStreamingResult<string>> Download(int id, string value);
    Task<DuplexStreamingResult<int, string>> Duplex();
}

[MetadataMarker("base-service")]
internal abstract class MetadataServiceBase : ServiceBase<IMetadataService>
{
    [MetadataMarker("base-method-1"), MetadataMarker("base-method-2")]
    public virtual UnaryResult<string> Greet(string name, int age = 7) => UnaryResult.FromResult(name);
}

[MetadataMarker("service")]
internal sealed class MetadataService : MetadataServiceBase, IMetadataService
{
    public List<string> Calls { get; } = [];
    public UnaryResult Ping() => default;
    public UnaryResult<int> Echo(int value) => UnaryResult.FromResult(value);
    [MetadataMarker("method")]
    public override UnaryResult<string> Greet(string name, int age = 18) => UnaryResult.FromResult($"{name}:{age}");
    public Task<ClientStreamingResult<int, string>> Upload()
    {
        Calls.Add("upload");
        return Task.FromResult(default(ClientStreamingResult<int, string>));
    }
    public Task<ServerStreamingResult<string>> Download(int id, string value)
    {
        Calls.Add($"{id}:{value}");
        return Task.FromResult(default(ServerStreamingResult<string>));
    }
    public Task<DuplexStreamingResult<int, string>> Duplex()
    {
        Calls.Add("duplex");
        return Task.FromResult(default(DuplexStreamingResult<int, string>));
    }
}

internal interface IExplicitMetadataService : IService<IExplicitMetadataService>
{
    UnaryResult<string> Echo(string value);
}

internal sealed class ExplicitMetadataService : ServiceBase<IExplicitMetadataService>, IExplicitMetadataService
{
    [MetadataMarker("explicit")]
    UnaryResult<string> IExplicitMetadataService.Echo(string value) => UnaryResult.FromResult(value);
}

internal interface IMetadataHubParent
{
    [MethodId(123), MetadataMarker("contract-query")]
    Task<int> Query(string name, int count = 99);
}

internal interface IMetadataHub : IStreamingHub<IMetadataHub, IMetadataReceiver>, IMetadataHubParent
{
    Task Notify();
    ValueTask Update(int value);
    void Send(string value);
}

internal interface IMetadataReceiver
{
    void OnMessage(string value);
}

[MetadataMarker("hub")]
internal sealed class MetadataHub : StreamingHubBase<IMetadataHub, IMetadataReceiver>, IMetadataHub
{
    [MetadataMarker("query")]
    public Task<int> Query(string name, int count = 9) => Task.FromResult(count);
    public Task Notify() => Task.CompletedTask;
    public ValueTask Update(int value) => default;
    public void Send(string value) { }
}
