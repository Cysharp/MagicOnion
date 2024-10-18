using Grpc.Core;
using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using System.Collections.Concurrent;
using System.Reflection;
using MagicOnion.Internal;
using MagicOnion.Server.Binder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server;

public interface IServiceContext
{
    Guid ContextId { get; }

    DateTime Timestamp { get; }

    Type ServiceType { get; }

    MethodInfo MethodInfo { get; }

    /// <summary>Cached Attributes both service and method.</summary>
    ILookup<Type, Attribute> AttributeLookup { get; }

    MethodType MethodType { get; }

    /// <summary>Raw gRPC Context.</summary>
    ServerCallContext CallContext { get; }

    IMagicOnionSerializer MessageSerializer { get; }

    IServiceProvider ServiceProvider { get; }

    ConcurrentDictionary<string, object> Items { get; }
}

public class ServiceContext : IServiceContext
{
    internal static AsyncLocal<ServiceContext?> currentServiceContext = new AsyncLocal<ServiceContext?>();

    /// <summary>
    /// Get Current ServiceContext. This property requires to MagicOnionOptions.Enable
    /// </summary>
    public static ServiceContext? Current => currentServiceContext.Value;

    ConcurrentDictionary<string, object>? items;
    object? request;

    /// <summary>Object storage per invoke.</summary>
    public ConcurrentDictionary<string, object> Items
    {
        get
        {
            lock (CallContext) // lock per CallContext, is this dangerous?
            {
                if (items == null) items = new ConcurrentDictionary<string, object>();
            }
            return items;
        }
    }

    public Guid ContextId { get; }

    public DateTime Timestamp { get; }

    public Type ServiceType => Method.ServiceImplementationType;

    public string ServiceName => Method.ServiceName;
    public string MethodName => MethodInfo.Name;

    public MethodInfo MethodInfo => Method.Metadata.ServiceMethod;

    /// <summary>Cached Attributes both service and method.</summary>
    public ILookup<Type, Attribute> AttributeLookup => Method.Metadata.AttributeLookup;

    public MethodType MethodType => Method.MethodType;

    /// <summary>Raw gRPC Context.</summary>
    public ServerCallContext CallContext { get; }

    public IMagicOnionSerializer MessageSerializer { get; }

    public IServiceProvider ServiceProvider { get; }

    internal object Instance { get; }
    internal object? Request => request;
    internal object? Result { get; set; }
    internal ILogger Logger { get; }
    internal IMagicOnionGrpcMethod Method { get; }
    internal MetricsContext Metrics { get; }

    internal ServiceContext(
        object instance,
        IMagicOnionGrpcMethod method,
        ServerCallContext context,
        IMagicOnionSerializer messageSerializer,
        MagicOnionMetrics metrics,
        ILogger logger,
        IServiceProvider serviceProvider
    )
    {
        this.ContextId = Guid.NewGuid();
        this.Instance = instance;
        this.CallContext = context;
        this.Timestamp = DateTime.UtcNow;
        this.MessageSerializer = messageSerializer;
        this.Logger = logger;
        this.Method = method;
        this.ServiceProvider = serviceProvider;
        this.Metrics = metrics.CreateContext();
    }

    /// <summary>Gets a request object.</summary>
    public object? GetRawRequest()
    {
        return request;
    }

    /// <summary>Sets a request object.</summary>
    public void SetRawRequest(object? request)
    {
        this.request = request;
    }

    /// <summary>Gets a response object. The object is available after the service method has completed.</summary>
    public object? GetRawResponse()
    {
        return Result;
    }

    /// <summary>Sets a response object. This can overwrite the result of the service method.</summary>
    public void SetRawResponse(object? response)
    {
        Result = response;
    }

    /// <summary>
    /// Sets a raw bytes response. The response will not be serialized and the bytes will be sent directly.
    /// This can overwrite the result of the service method.
    /// </summary>
    public void SetRawBytesResponse(ReadOnlyMemory<byte> bytes)
    {
        Result = new RawBytesBox(bytes);
    }
}
