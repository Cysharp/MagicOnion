using System.Reflection;
using Grpc.Core;

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

internal class MethodHandlerMetadataFactory
{
    public static MethodHandlerMetadata Create(Type serviceClass, MethodInfo methodInfo)
    {
        var serviceInterfaceType = serviceClass.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>)).GetGenericArguments()[0];
        var parameters = methodInfo.GetParameters();
        var responseType = UnwrapResponseType(methodInfo, out var methodType, out var responseIsTask, out var requestTypeIfExists);
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

    static Type UnwrapResponseType(MethodInfo methodInfo, out MethodType methodType, out bool responseIsTask, out Type? requestTypeIfExists)
    {
        var t = methodInfo.ReturnType;
        if (!t.GetTypeInfo().IsGenericType) throw new InvalidOperationException($"A method has invalid return type. (Member:{methodInfo.DeclaringType!.Name}.{methodInfo.Name}, ReturnType:{methodInfo.ReturnType.Name})");

        // Task<Unary<T>>
        if (t.GetGenericTypeDefinition() == typeof(Task<>))
        {
            responseIsTask = true;
            t = t.GetGenericArguments()[0];
        }
        else
        {
            responseIsTask = false;
        }

        // Unary<T>
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
        else
        {
            throw new InvalidOperationException($"Invalid return type, path:{methodInfo.DeclaringType!.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");
        }
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
            default: throw new InvalidOperationException($"A method '{methodInfo.Name}' has too many parameters. The method must have less than 16 parameters. (Length: {parameterTypes.Length})");
        }
    }
}
