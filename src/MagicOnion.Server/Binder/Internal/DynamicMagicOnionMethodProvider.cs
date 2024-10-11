using System.Linq.Expressions;
using System.Reflection;
using MagicOnion.Internal;
using MagicOnion.Server.Hubs;

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
            var typeRequest = CreateRequestType(methodParameters);
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

            // (instance, context, request) => instance.Foo(request.Item1, request.Item2...);
            var exprParamInstance = Expression.Parameter(typeServiceImplementation);
            var exprParamServiceContext = Expression.Parameter(typeof(ServiceContext));
            var exprParamRequest = Expression.Parameter(typeRequest);
            var exprArguments = methodParameters.Length == 1
                ? [exprParamRequest]
                : methodParameters
                    .Select((x, i) => Expression.Field(exprParamRequest, "Item" + (i + 1)))
                    .Cast<Expression>()
                    .ToArray();

            var exprCall = Expression.Call(exprParamInstance, targetMethod, exprArguments);
            var invoker = Expression.Lambda(exprCall, [exprParamInstance, exprParamServiceContext, exprParamRequest]).Compile();

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
            var typeRequest = CreateRequestType(methodParameters);
            var typeResponse = methodInfo.ReturnType;

            Type hubMethodType;
            if (typeResponse == typeof(ValueTask) || typeResponse == typeof(Task) || typeResponse == typeof(void))
            {
                hubMethodType = typeof(MagicOnionStreamingHubMethod<,>).MakeGenericType([typeServiceImplementation, typeRequest]);
            }
            else if (typeResponse.IsConstructedGenericType && typeResponse.GetGenericTypeDefinition() is {} typeResponseOpen && (typeResponseOpen == typeof(Task<>) || typeResponseOpen == typeof(ValueTask<>)))
            {
                hubMethodType = typeof(MagicOnionStreamingHubMethod<,,>).MakeGenericType([typeServiceImplementation, typeRequest, typeResponse.GetGenericArguments()[0]]);
            }
            else
            {
                throw new InvalidOperationException("Unsupported method return type. The return type of StreamingHub method must be one of 'void', 'Task', 'ValueTask', 'Task<T>' or 'ValueTask<T>'.");
            }

            // Invoker
            // var invokeHubMethodFunc = (service, context, request) => service.Foo(request);
            // or
            // var invokeHubMethodFunc = (service, context, request) => service.Foo(request.Item1, request.Item2 ...);
            var exprParamService = Expression.Parameter(typeof(TService), "service");
            var exprParamContext = Expression.Parameter(typeof(StreamingHubContext), "context");
            var exprParamRequest = Expression.Parameter(typeRequest, "request");
            var exprArguments = methodParameters.Length == 1
                ? [exprParamRequest]
                : methodParameters
                    .Select((x, i) => Expression.Field(exprParamRequest, "Item" + (i + 1)))
                    .Cast<Expression>()
                    .ToArray();

            var exprCallHubMethod = Expression.Call(exprParamService, methodInfo, exprArguments);
            var invoker = Expression.Lambda(exprCallHubMethod, [exprParamService, exprParamContext, exprParamRequest]).Compile();

            var hubMethod = (IMagicOnionStreamingHubMethod)Activator.CreateInstance(hubMethodType, [typeServiceInterface.Name, methodInfo.Name, invoker])!;
            yield return hubMethod;
        }
    }

    static Type CreateRequestType(ParameterInfo[] parameters)
    {
        return parameters.Length switch
        {
            0 => typeof(MessagePack.Nil),
            1 => parameters[0].ParameterType,
            2 => typeof(DynamicArgumentTuple<,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType),
            3 => typeof(DynamicArgumentTuple<,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType),
            4 => typeof(DynamicArgumentTuple<,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType),
            5 => typeof(DynamicArgumentTuple<,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType),
            6 => typeof(DynamicArgumentTuple<,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType),
            7 => typeof(DynamicArgumentTuple<,,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType, parameters[6].ParameterType),
            8 => typeof(DynamicArgumentTuple<,,,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType, parameters[6].ParameterType, parameters[7].ParameterType),
            9 => typeof(DynamicArgumentTuple<,,,,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType, parameters[6].ParameterType, parameters[7].ParameterType, parameters[8].ParameterType),
            10 => typeof(DynamicArgumentTuple<,,,,,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType, parameters[6].ParameterType, parameters[7].ParameterType, parameters[8].ParameterType, parameters[9].ParameterType),
            11 => typeof(DynamicArgumentTuple<,,,,,,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType, parameters[6].ParameterType, parameters[7].ParameterType, parameters[8].ParameterType, parameters[9].ParameterType, parameters[10].ParameterType),
            12 => typeof(DynamicArgumentTuple<,,,,,,,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType, parameters[6].ParameterType, parameters[7].ParameterType, parameters[8].ParameterType, parameters[9].ParameterType, parameters[10].ParameterType, parameters[11].ParameterType),
            13 => typeof(DynamicArgumentTuple<,,,,,,,,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType, parameters[6].ParameterType, parameters[7].ParameterType, parameters[8].ParameterType, parameters[9].ParameterType, parameters[10].ParameterType, parameters[11].ParameterType, parameters[12].ParameterType),
            14 => typeof(DynamicArgumentTuple<,,,,,,,,,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType, parameters[6].ParameterType, parameters[7].ParameterType, parameters[8].ParameterType, parameters[9].ParameterType, parameters[10].ParameterType, parameters[11].ParameterType, parameters[12].ParameterType, parameters[13].ParameterType),
            15 => typeof(DynamicArgumentTuple<,,,,,,,,,,,,,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType, parameters[4].ParameterType, parameters[5].ParameterType, parameters[6].ParameterType, parameters[7].ParameterType, parameters[8].ParameterType, parameters[9].ParameterType, parameters[10].ParameterType, parameters[11].ParameterType, parameters[12].ParameterType, parameters[13].ParameterType, parameters[14].ParameterType),
            _ =>  throw new NotSupportedException("The method must have no more than 16 parameters."),
        };
    }
}
