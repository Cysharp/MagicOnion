using System.Diagnostics;

namespace MagicOnion.Server.SourceGenerator.CodeAnalysis;

/// <summary>
/// Represents information about a MagicOnion service implementation class.
/// </summary>
[DebuggerDisplay("Service: {ImplementationType.FullName,nq} -> {ServiceInterfaceType.FullName,nq}")]
public class ServiceImplementationInfo
{
    /// <summary>
    /// Gets the service implementation class type (e.g., GreeterService).
    /// </summary>
    public MagicOnionTypeInfo ImplementationType { get; }

    /// <summary>
    /// Gets the service interface type (e.g., IGreeterService).
    /// </summary>
    public MagicOnionTypeInfo ServiceInterfaceType { get; }

    /// <summary>
    /// Gets whether this is a StreamingHub service.
    /// </summary>
    public bool IsStreamingHub { get; }

    /// <summary>
    /// Gets the receiver interface type for StreamingHub (e.g., IGreeterHubReceiver).
    /// Only valid when IsStreamingHub is true.
    /// </summary>
    public MagicOnionTypeInfo? ReceiverInterfaceType { get; }

    /// <summary>
    /// Gets the list of service methods.
    /// </summary>
    public IReadOnlyList<ServiceMethodInfo> Methods { get; }

    /// <summary>
    /// Gets the list of StreamingHub methods.
    /// Only valid when IsStreamingHub is true.
    /// </summary>
    public IReadOnlyList<StreamingHubMethodInfo> HubMethods { get; }

    public ServiceImplementationInfo(
        MagicOnionTypeInfo implementationType,
        MagicOnionTypeInfo serviceInterfaceType,
        bool isStreamingHub,
        MagicOnionTypeInfo? receiverInterfaceType,
        IReadOnlyList<ServiceMethodInfo> methods,
        IReadOnlyList<StreamingHubMethodInfo> hubMethods)
    {
        ImplementationType = implementationType;
        ServiceInterfaceType = serviceInterfaceType;
        IsStreamingHub = isStreamingHub;
        ReceiverInterfaceType = receiverInterfaceType;
        Methods = methods;
        HubMethods = hubMethods;
    }
}

/// <summary>
/// Represents information about a service method (Unary, ClientStreaming, ServerStreaming, DuplexStreaming).
/// </summary>
[DebuggerDisplay("Method: {MethodName,nq} ({MethodType})")]
public class ServiceMethodInfo
{
    public MethodType MethodType { get; }
    public string ServiceName { get; }
    public string MethodName { get; }
    public IReadOnlyList<MagicOnionMethodParameterInfo> Parameters { get; }
    public MagicOnionTypeInfo ReturnType { get; }
    public MagicOnionTypeInfo RequestType { get; }
    public MagicOnionTypeInfo ResponseType { get; }

    public ServiceMethodInfo(
        MethodType methodType,
        string serviceName,
        string methodName,
        IReadOnlyList<MagicOnionMethodParameterInfo> parameters,
        MagicOnionTypeInfo returnType,
        MagicOnionTypeInfo requestType,
        MagicOnionTypeInfo responseType)
    {
        MethodType = methodType;
        ServiceName = serviceName;
        MethodName = methodName;
        Parameters = parameters;
        ReturnType = returnType;
        RequestType = requestType;
        ResponseType = responseType;
    }
}

/// <summary>
/// Represents information about a StreamingHub method.
/// </summary>
[DebuggerDisplay("HubMethod: {MethodName,nq} (Id={MethodId})")]
public class StreamingHubMethodInfo
{
    public int MethodId { get; }
    public string MethodName { get; }
    public IReadOnlyList<MagicOnionMethodParameterInfo> Parameters { get; }
    public MagicOnionTypeInfo ReturnType { get; }
    public MagicOnionTypeInfo RequestType { get; }
    public MagicOnionTypeInfo ResponseType { get; }

    public StreamingHubMethodInfo(
        int methodId,
        string methodName,
        IReadOnlyList<MagicOnionMethodParameterInfo> parameters,
        MagicOnionTypeInfo returnType,
        MagicOnionTypeInfo requestType,
        MagicOnionTypeInfo responseType)
    {
        MethodId = methodId;
        MethodName = methodName;
        Parameters = parameters;
        ReturnType = returnType;
        RequestType = requestType;
        ResponseType = responseType;
    }
}
