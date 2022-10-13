using Grpc.Core;
using MagicOnion.Server.Diagnostics;
using MessagePack;
using System.Collections.Concurrent;
using System.Reflection;

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

    IMagicOnionMessageSerializer MessageSerializer { get; }

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

    public Type ServiceType { get; }

    public MethodInfo MethodInfo { get; }

    /// <summary>Cached Attributes both service and method.</summary>
    public ILookup<Type, Attribute> AttributeLookup { get; }

    public MethodType MethodType { get; }

    /// <summary>Raw gRPC Context.</summary>
    public ServerCallContext CallContext { get; }

    public IMagicOnionMessageSerializer MessageSerializer { get; private set; }

    public IServiceProvider ServiceProvider { get; }

    internal object? Request => request;
    internal object? Result { get; set; }
    internal IMagicOnionLogger MagicOnionLogger { get; }
    internal MethodHandler MethodHandler { get; }

    public ServiceContext(
        Type serviceType,
        MethodInfo methodInfo,
        ILookup<Type, Attribute> attributeLookup,
        MethodType methodType,
        ServerCallContext context,
        IMagicOnionMessageSerializer messageSerializer,
        IMagicOnionLogger logger,
        MethodHandler methodHandler,
        IServiceProvider serviceProvider
    )
    {
        this.ContextId = Guid.NewGuid();
        this.ServiceType = serviceType;
        this.MethodInfo = methodInfo;
        this.AttributeLookup = attributeLookup;
        this.MethodType = methodType;
        this.CallContext = context;
        this.Timestamp = DateTime.UtcNow;
        this.MessageSerializer = messageSerializer;
        this.MagicOnionLogger = logger;
        this.MethodHandler = methodHandler;
        this.ServiceProvider = serviceProvider;
    }

    /// <summary>Get Raw Request.</summary>
    public object? GetRawRequest()
    {
        return request;
    }

    /// <summary>Set Raw Request, you can set before method body was called.</summary>
    public void SetRawRequest(object? request)
    {
        this.request = request;
    }

    /// <summary>Can get after method body was finished.</summary>
    public object? GetRawResponse()
    {
        return Result;
    }

    /// <summary>Can set after method body was finished.</summary>
    public void SetRawResponse(object? response)
    {
        Result = response;
    }

    /// <summary>
    /// modify request/response options in this context.
    /// </summary>
    public void ChangeSerializer(IMagicOnionMessageSerializer messageSerializer)
    {
        this.MessageSerializer = messageSerializer;
    }
}
