using System.Linq.Expressions;
using System.Reflection;
using MagicOnion.Internal;

namespace MagicOnion.Server.Binder.Internal;

internal class DynamicMagicOnionMethodProvider : IMagicOnionGrpcMethodProvider
{
    readonly MagicOnionServiceDefinition definition;

    public DynamicMagicOnionMethodProvider(MagicOnionServiceDefinition definition)
    {
        this.definition = definition;
    }

    public void OnRegisterGrpcServices(MagicOnionGrpcServiceRegistrationContext context)
    {
        foreach (var serviceType in this.definition.TargetTypes.Distinct())
        {
            context.Register(serviceType);
        }
    }

    public IEnumerable<IMagicOnionGrpcMethod> GetGrpcMethods<TService>() where TService : class
    {
        var typeServiceImplementation = typeof(TService);
        var typeServiceInterface = typeServiceImplementation.GetInterfaces()
            .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>))
            .GenericTypeArguments[0];

        // StreamingHub
        if (typeof(TService).IsAssignableTo(typeof(IStreamingHubMarker)))
        {
            yield return new MagicOnionStreamingHubConnectMethod<TService>(typeServiceInterface.Name);
            yield break;
        }

        // Unary, ClientStreaming, ServerStreaming, DuplexStreaming
        var interfaceMap = typeServiceImplementation.GetInterfaceMapWithParents(typeServiceInterface);
        for (var i = 0; i < interfaceMap.TargetMethods.Length; i++)
        {
            var methodInfo = interfaceMap.TargetMethods[i];
            var methodName = interfaceMap.InterfaceMethods[i].Name;

            if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) continue;
            if (methodInfo.GetCustomAttribute<IgnoreAttribute>(false) != null) continue; // ignore
            if (methodName is "Equals" or "GetHashCode" or "GetType" or "ToString" or "WithOptions" or "WithHeaders" or "WithDeadline" or "WithCancellationToken" or "WithHost") continue;

            var targetMethod = methodInfo;
            var methodParameters = targetMethod.GetParameters();
            var typeRequest = methodParameters is { Length: > 1 }
                ? typeof(DynamicArgumentTuple<,>).MakeGenericType(methodParameters[0].ParameterType, methodParameters[1].ParameterType)
                : methodParameters is { Length: 1 }
                    ? methodParameters[0].ParameterType
                    : typeof(MessagePack.Nil);
            var typeRawRequest = typeRequest.IsValueType
                ? typeof(Box<>).MakeGenericType(typeRequest)
                : typeRequest;

            Type typeMethod;
            if (targetMethod.ReturnType == typeof(UnaryResult))
            {
                // UnaryResult: The method has no return value.
                typeMethod = typeof(MagicOnionUnaryMethod<,,>).MakeGenericType(typeServiceImplementation, typeRequest, typeRawRequest);
            }
            else
            {
                // UnaryResult<T>
                var typeResponse = targetMethod.ReturnType.GetGenericArguments()[0];
                var typeRawResponse = typeResponse.IsValueType
                    ? typeof(Box<>).MakeGenericType(typeResponse)
                    : typeResponse;
                typeMethod = typeof(MagicOnionUnaryMethod<,,,,>).MakeGenericType(typeServiceImplementation, typeRequest, typeResponse, typeRawRequest, typeRawResponse);
            }

            var exprParamInstance = Expression.Parameter(typeServiceImplementation);
            var exprParamRequest = Expression.Parameter(typeRequest);
            var exprParamServiceContext = Expression.Parameter(typeof(ServiceContext));
            var exprArguments = methodParameters.Length == 1
                ? [exprParamRequest]
                : methodParameters
                    .Select((x, i) => Expression.Field(exprParamRequest, "Item" + (i + 1)))
                    .Cast<Expression>()
                    .ToArray();

            var exprCall = Expression.Call(exprParamInstance, targetMethod, exprArguments);
            var invoker = Expression.Lambda(exprCall, [exprParamInstance, exprParamRequest, exprParamServiceContext]).Compile();

            var serviceMethod = Activator.CreateInstance(typeMethod, [typeServiceInterface.Name, targetMethod.Name, invoker])!;
            yield return (IMagicOnionGrpcMethod)serviceMethod;
        }
    }

    public IEnumerable<IMagicOnionStreamingHubMethod> GetStreamingHubMethods<TService>() where TService : class
    {
        if (!typeof(TService).IsAssignableTo(typeof(IStreamingHubMarker)))
        {
            yield break;
        }

        var typeServiceImplementation = typeof(TService);
        var typeServiceInterface = typeServiceImplementation.GetInterfaces()
            .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>))
            .GenericTypeArguments[0];

        var interfaceMap = typeServiceImplementation.GetInterfaceMapWithParents(typeServiceInterface);
        for (var i = 0; i < interfaceMap.TargetMethods.Length; i++)
        {
            var methodInfo = interfaceMap.TargetMethods[i];
            var methodName = interfaceMap.InterfaceMethods[i].Name;

            if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) continue;
            if (methodInfo.GetCustomAttribute<IgnoreAttribute>(false) != null) continue; // ignore
            if (methodName is "Equals" or "GetHashCode" or "GetType" or "ToString" or "WithOptions" or "WithHeaders" or "WithDeadline" or "WithCancellationToken" or "WithHost") continue;

            var methodParameters = methodInfo.GetParameters();
            var typeRequest = methodParameters is { Length: > 1 }
                ? typeof(DynamicArgumentTuple<,>).MakeGenericType(methodParameters[0].ParameterType, methodParameters[1].ParameterType)
                : methodParameters is { Length: 1 }
                    ? methodParameters[0].ParameterType
                    : typeof(MessagePack.Nil);
            var typeResponse = methodInfo.ReturnType;

            Type hubMethodType;
            if (typeResponse == typeof(ValueTask) || typeResponse == typeof(Task) || typeResponse == typeof(void))
            {
                hubMethodType = typeof(MagicOnionStreamingHubMethod<,>).MakeGenericType([typeServiceImplementation, typeRequest]);
            }
            else
            {
                hubMethodType = typeof(MagicOnionStreamingHubMethod<,,>).MakeGenericType([typeServiceImplementation, typeRequest, typeResponse]);
            }

            yield return (IMagicOnionStreamingHubMethod)Activator.CreateInstance(hubMethodType, [typeServiceInterface.Name, methodInfo.Name, methodInfo])!;
        }
    }
}
