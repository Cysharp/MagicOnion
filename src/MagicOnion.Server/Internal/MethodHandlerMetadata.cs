using System.Reflection;
using Grpc.Core;
using MagicOnion.Server.Hubs;
using MagicOnion.Utils;

namespace MagicOnion.Server.Internal;

public readonly struct MethodHandlerMetadata
{
    public Type ServiceImplementationType { get; }
    public MethodInfo ServiceMethod { get; }

    public MethodType MethodType { get; }
    public Type ResponseType { get; }
    public Type RequestType { get; }
    public IReadOnlyList<ParameterInfo> Parameters { get; }
    public Type ServiceInterface { get; }
    public ILookup<Type, Attribute> AttributeLookup { get; }
    public bool IsResultTypeTask { get; }

    public MethodHandlerMetadata(
        Type serviceImplementationType,
        MethodInfo serviceMethod,
        MethodType methodType,
        Type responseType,
        Type requestType,
        IReadOnlyList<ParameterInfo> parameters,
        Type serviceInterface,
        ILookup<Type, Attribute> attributeLookup,
        bool isResultTypeTask
    )
    {
        ServiceImplementationType = serviceImplementationType;
        ServiceMethod = serviceMethod;

        MethodType = methodType;
        ResponseType = responseType;
        RequestType = requestType;
        Parameters = parameters;
        ServiceInterface = serviceInterface;
        AttributeLookup = attributeLookup;
        IsResultTypeTask = isResultTypeTask;
    }
}

public readonly struct StreamingHubMethodHandlerMetadata
{
    public int MethodId { get; }
    public Type StreamingHubImplementationType { get; }
    public Type StreamingHubInterfaceType { get; }
    public MethodInfo InterfaceMethod { get; }
    public MethodInfo ImplementationMethod { get; }
    public Type? ResponseType { get; }
    public Type RequestType { get; }
    public IReadOnlyList<ParameterInfo> Parameters { get; }
    public ILookup<Type, Attribute> AttributeLookup { get; }

    public StreamingHubMethodHandlerMetadata(int methodId, Type streamingHubImplementationType, MethodInfo interfaceMethodInfo, MethodInfo implementationMethodInfo, Type? responseType, Type requestType, IReadOnlyList<ParameterInfo> parameters, Type streamingHubInterfaceType, ILookup<Type, Attribute> attributeLookup)
    {
        MethodId = methodId;
        StreamingHubImplementationType = streamingHubImplementationType;
        InterfaceMethod = interfaceMethodInfo;
        ImplementationMethod = implementationMethodInfo;
        ResponseType = responseType;
        RequestType = requestType;
        Parameters = parameters;
        StreamingHubInterfaceType = streamingHubInterfaceType;
        AttributeLookup = attributeLookup;
    }
}

internal class MethodHandlerMetadataFactory
{
    public static MethodHandlerMetadata CreateServiceMethodHandlerMetadata(Type serviceClass, MethodInfo methodInfo)
    {
        var serviceInterfaceType = serviceClass.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>)).GetGenericArguments()[0];
        var parameters = methodInfo.GetParameters();
        var responseType = UnwrapUnaryResponseType(methodInfo, out var methodType, out var responseIsTask, out var requestTypeIfExists);
        var requestType = requestTypeIfExists ?? GetRequestTypeFromMethod(methodInfo, parameters);

        var attributeLookup = serviceClass.GetCustomAttributes(true)
            .Concat(methodInfo.GetCustomAttributes(true))
            .Cast<Attribute>()
            .ToLookup(x => x.GetType());

        if (parameters.Any() && methodType is MethodType.ClientStreaming or MethodType.DuplexStreaming)
        {
            throw new InvalidOperationException($"{methodType} does not support method parameters. If you need to send some arguments, use request headers instead. (Member:{serviceClass.Name}.{methodInfo.Name})");
        }

        return new MethodHandlerMetadata(serviceClass, methodInfo, methodType, responseType, requestType, parameters, serviceInterfaceType, attributeLookup, responseIsTask);
    }

    public static StreamingHubMethodHandlerMetadata CreateStreamingHubMethodHandlerMetadata(Type serviceClass, MethodInfo methodInfo)
    {
        var hubInterface = serviceClass.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IStreamingHub<,>)).GetGenericArguments()[0];
        var parameters = methodInfo.GetParameters();
        var responseType = UnwrapStreamingHubResponseType(methodInfo, out var responseIsTaskOrValueTask);
        var requestType = GetRequestTypeFromMethod(methodInfo, parameters);

        var attributeLookup = serviceClass.GetCustomAttributes(true)
            .Concat(methodInfo.GetCustomAttributes(true))
            .Cast<Attribute>()
            .ToLookup(x => x.GetType());

        var interfaceMethodInfo = ResolveInterfaceMethod(serviceClass, hubInterface, methodInfo.Name);

        if (!responseIsTaskOrValueTask)
        {
            throw new InvalidOperationException($"A type of the StreamingHub method must be Task, Task<T>, ValueTask or ValueTask<T>. (Member:{serviceClass.Name}.{methodInfo.Name})");
        }

        var methodId = interfaceMethodInfo.GetCustomAttribute<MethodIdAttribute>()?.MethodId ?? FNV1A32.GetHashCode(interfaceMethodInfo.Name);
        if (methodInfo.GetCustomAttribute<MethodIdAttribute>() is not null)
        {
            throw new InvalidOperationException($"The '{serviceClass.Name}.{methodInfo.Name}' cannot have MethodId attribute. MethodId attribute must be annotated to a hub interface instead.");
        }

        return new StreamingHubMethodHandlerMetadata(methodId, serviceClass, interfaceMethodInfo, methodInfo, responseType, requestType, parameters, hubInterface, attributeLookup);
    }

    static MethodInfo ResolveInterfaceMethod(Type targetType, Type interfaceType, string targetMethodName)
    {
        var mapping = targetType.GetInterfaceMapWithParents(interfaceType);
        var methodIndex = Array.FindIndex(mapping.TargetMethods, mi => mi.Name == targetMethodName);
        return mapping.InterfaceMethods[methodIndex];
    }

    static Type UnwrapUnaryResponseType(MethodInfo methodInfo, out MethodType methodType, out bool responseIsTask, out Type? requestTypeIfExists)
    {
        var t = methodInfo.ReturnType;
        
        // UnaryResult
        if (t == typeof(UnaryResult))
        {
            methodType = MethodType.Unary;
            requestTypeIfExists = default;
            responseIsTask = false;
            return typeof(MessagePack.Nil);
        }

        if (!t.GetTypeInfo().IsGenericType)
        {
            throw new InvalidOperationException($"The method '{methodInfo.Name}' has invalid return type. (Member:{methodInfo.DeclaringType!.Name}.{methodInfo.Name}, ReturnType:{methodInfo.ReturnType.Name})");
        }

        // Task<UnaryResult<T>>
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>))
        {
            responseIsTask = true;
            t = t.GetGenericArguments()[0];
        }
        else
        {
            responseIsTask = false;
        }

        // UnaryResult<T>, ClientStreamingResult<TRequest,TResponse>, ServerStreamingResult<T>, DuplexStreamingResult<TRequest,TResponse>
        var returnType = t.GetGenericTypeDefinition();
        if (returnType == typeof(UnaryResult<>))
        {
            methodType = MethodType.Unary;
            requestTypeIfExists = default;
            return t.GetGenericArguments()[0];
        }
        else if (returnType == typeof(ClientStreamingResult<,>))
        {
            methodType = MethodType.ClientStreaming;
            var genArgs = t.GetGenericArguments();
            requestTypeIfExists = genArgs[0];
            return genArgs[1];
        }
        else if (returnType == typeof(ServerStreamingResult<>))
        {
            methodType = MethodType.ServerStreaming;
            requestTypeIfExists = default;
            return t.GetGenericArguments()[0];
        }
        else if (returnType == typeof(DuplexStreamingResult<,>))
        {
            methodType = MethodType.DuplexStreaming;
            var genArgs = t.GetGenericArguments();
            requestTypeIfExists = genArgs[0];
            return genArgs[1];
        }

        throw new InvalidOperationException($"The method '{methodInfo.Name}' has invalid return type. path:{methodInfo.DeclaringType!.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");
    }

    static Type? UnwrapStreamingHubResponseType(MethodInfo methodInfo, out bool responseIsTaskOrValueTask)
    {
        var t = methodInfo.ReturnType;

        // Task<T>
        if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Task<>) || t.GetGenericTypeDefinition() == typeof(ValueTask<>)))
        {
            responseIsTaskOrValueTask = true;
            return t.GetGenericArguments()[0];
        }
        else if (t == typeof(Task) || t == typeof(ValueTask))
        {
            responseIsTaskOrValueTask = true;
            return null;
        }

        throw new InvalidOperationException($"The method '{methodInfo.Name}' has invalid return type. path:{methodInfo.DeclaringType!.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");
    }

    /// <summary>
    /// Gets the type of wrapped request parameters from the method parameters.
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    static Type GetRequestTypeFromMethod(MethodInfo methodInfo, IReadOnlyList<ParameterInfo> parameters)
    {
        var parameterTypes = parameters.Select(x => x.ParameterType).ToArray();
        switch (parameterTypes.Length)
        {
            case 0: return typeof(MessagePack.Nil);
            case 1: return parameterTypes[0];
            case 2: return typeof(DynamicArgumentTuple<,>).MakeGenericType(parameterTypes[0], parameterTypes[1]);
            case 3: return typeof(DynamicArgumentTuple<,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2]);
            case 4: return typeof(DynamicArgumentTuple<,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3]);
            case 5: return typeof(DynamicArgumentTuple<,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4]);
            case 6: return typeof(DynamicArgumentTuple<,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5]);
            case 7: return typeof(DynamicArgumentTuple<,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6]);
            case 8: return typeof(DynamicArgumentTuple<,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7]);
            case 9: return typeof(DynamicArgumentTuple<,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8]);
            case 10: return typeof(DynamicArgumentTuple<,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9]);
            case 11: return typeof(DynamicArgumentTuple<,,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10]);
            case 12: return typeof(DynamicArgumentTuple<,,,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10], parameterTypes[11]);
            case 13: return typeof(DynamicArgumentTuple<,,,,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10], parameterTypes[11], parameterTypes[12]);
            case 14: return typeof(DynamicArgumentTuple<,,,,,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10], parameterTypes[11], parameterTypes[12], parameterTypes[13]);
            case 15: return typeof(DynamicArgumentTuple<,,,,,,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10], parameterTypes[11], parameterTypes[12], parameterTypes[13], parameterTypes[14]);
            default: throw new InvalidOperationException($"The method '{methodInfo.Name}' has too many parameters. The method must have less than 16 parameters. (Length: {parameterTypes.Length})");
        }
    }
}
