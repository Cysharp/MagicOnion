using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Client.DynamicClient
{
    internal class ServiceClientDefinition
    {
        public Type ServiceInterfaceType { get; }
        public IReadOnlyList<MagicOnionServiceMethodInfo> Methods { get; }

        public ServiceClientDefinition(Type serviceInterfaceType, IReadOnlyList<MagicOnionServiceMethodInfo> methods)
        {
            ServiceInterfaceType = serviceInterfaceType;
            Methods = methods;
        }

        public class MagicOnionServiceMethodInfo
        {
            public MethodType MethodType { get; }
            public string ServiceName { get; }
            public string MethodName { get; }
            public string Path { get; }
            public IReadOnlyList<Type> ParameterTypes { get; }
            public Type MethodReturnType { get; }
            public Type RequestType { get; }
            public Type ResponseType { get; }

            public MagicOnionServiceMethodInfo(MethodType methodType, string serviceName, string methodName, string path, IReadOnlyList<Type> parameterTypes, Type methodReturnType, Type requestType, Type responseType)
            {
                Debug.Assert(requestType != typeof(void));
                Debug.Assert(responseType != typeof(void));
                Debug.Assert(!(responseType.IsConstructedGenericType && (responseType.GetGenericTypeDefinition() == typeof(UnaryResult<>) || 
                                                                         responseType.GetGenericTypeDefinition() == typeof(ClientStreamingResult<,>) ||
                                                                         responseType.GetGenericTypeDefinition() == typeof(ServerStreamingResult<>) ||
                                                                         responseType.GetGenericTypeDefinition() == typeof(DuplexStreamingResult<,>))));

                MethodType = methodType;
                ServiceName = serviceName;
                MethodName = methodName;
                Path = path;
                ParameterTypes = parameterTypes;
                MethodReturnType = methodReturnType;
                RequestType = requestType;
                ResponseType = responseType;
            }

            public static MagicOnionServiceMethodInfo Create(Type serviceType, MethodInfo methodInfo)
            {
                var (methodType, requestType, responseType) = GetMethodTypeAndResponseTypeFromMethod(methodInfo);

                var method = new MagicOnionServiceMethodInfo(
                    methodType,
                    serviceType.Name,
                    methodInfo.Name,
                    $"{serviceType.Name}/{methodInfo.Name}",
                    methodInfo.GetParameters().Select(y => y.ParameterType).ToArray(),
                    methodInfo.ReturnType,
                    requestType ?? GetRequestTypeFromMethod(methodInfo),
                    responseType
                );
                method.Verify();

                return method;
            }

            private void Verify()
            {
                switch (MethodType)
                {
                    case MethodType.Unary:
                        if ((MethodReturnType != typeof(UnaryResult)) &&
                            (MethodReturnType.IsGenericType && MethodReturnType.GetGenericTypeDefinition() != typeof(UnaryResult<>)))
                        {
                            throw new InvalidOperationException($"The return type of Unary method must be UnaryResult<T>. (Service: {ServiceName}, Method: {MethodName})");
                        }
                        break;
                    case MethodType.ClientStreaming:
                        break;
                    case MethodType.DuplexStreaming:
                        break;
                    case MethodType.ServerStreaming:
                        break;
                    default:
                        throw new NotSupportedException(); // Unreachable
                }
            }

            private static (MethodType MethodType, Type? RequestType, Type ResponseType) GetMethodTypeAndResponseTypeFromMethod(MethodInfo methodInfo)
            {
                const string UnsupportedReturnTypeErrorMessage =
                    "The method of a service must return 'UnaryResult<T>', 'Task<ClientStreamingResult<TRequest, TResponse>>', 'Task<ServerStreamingResult<T>>' or 'DuplexStreamingResult<TRequest, TResponse>'.";

                var returnType = methodInfo.ReturnType;
                if (returnType == typeof(UnaryResult))
                {
                    return (MethodType.Unary, null, typeof(Nil));
                }
                if (!returnType.IsGenericType)
                {
                    throw new InvalidOperationException($"{UnsupportedReturnTypeErrorMessage} (Method: {methodInfo.DeclaringType!.Name}.{methodInfo.Name})");
                }

                var isTaskOfT = false;
                var returnTypeOpen = returnType.GetGenericTypeDefinition();
                if (returnTypeOpen == typeof(Task<>))
                {
                    isTaskOfT = true;
                    returnType = returnType.GetGenericArguments()[0];
                    if (!returnType.IsGenericType)
                    {
                        throw new InvalidOperationException($"{UnsupportedReturnTypeErrorMessage} (Method: {methodInfo.DeclaringType!.Name}.{methodInfo.Name})");
                    }
                    returnTypeOpen = returnType.GetGenericTypeDefinition();
                }

                if (returnTypeOpen == typeof(UnaryResult))
                {
                    if (isTaskOfT)
                    {
                        throw new InvalidOperationException($"The return type of an Unary method must be 'UnaryResult' or 'UnaryResult<T>'. (Method: {methodInfo.DeclaringType!.Name}.{methodInfo.Name})");
                    }
                    return (MethodType.Unary, null, typeof(Nil));
                }
                else if (returnTypeOpen == typeof(UnaryResult<>))
                {
                    if (isTaskOfT)
                    {
                        throw new InvalidOperationException($"The return type of an Unary method must be 'UnaryResult' or 'UnaryResult<T>'. (Method: {methodInfo.DeclaringType!.Name}.{methodInfo.Name})");
                    }
                    return (MethodType.Unary, null, returnType.GetGenericArguments()[0]);
                }
                else if (returnTypeOpen == typeof(ClientStreamingResult<,>))
                {
                    if (!isTaskOfT)
                    {
                        throw new InvalidOperationException($"The return type of a Streaming method must be 'Task<T>'. (Method: {methodInfo.DeclaringType!.Name}.{methodInfo.Name})");
                    }
                    return (MethodType.ClientStreaming, returnType.GetGenericArguments()[0], returnType.GetGenericArguments()[1]);
                }
                else if (returnTypeOpen == typeof(ServerStreamingResult<>))
                {
                    if (!isTaskOfT)
                    {
                        throw new InvalidOperationException($"The return type of a Streaming method must be 'Task<T>'. (Method: {methodInfo.DeclaringType!.Name}.{methodInfo.Name})");
                    }
                    return (MethodType.ServerStreaming, null, returnType.GetGenericArguments()[0]); // Use method parameters as response type
                }
                else if (returnTypeOpen == typeof(DuplexStreamingResult<,>))
                {
                    if (!isTaskOfT)
                    {
                        throw new InvalidOperationException($"The return type of a Streaming method must be 'Task<T>'. (Method: {methodInfo.DeclaringType!.Name}.{methodInfo.Name})");
                    }
                    return (MethodType.DuplexStreaming, returnType.GetGenericArguments()[0], returnType.GetGenericArguments()[1]);
                }
                else
                {
                    throw new InvalidOperationException($"{UnsupportedReturnTypeErrorMessage} (Method: {methodInfo.DeclaringType!.Name}.{methodInfo.Name})");
                }
            }
        }

        public static ServiceClientDefinition CreateFromType<T>()
        {
            return new ServiceClientDefinition(typeof(T), GetServiceMethods(typeof(T)));
        }
        
        private static IReadOnlyList<MagicOnionServiceMethodInfo> GetServiceMethods(Type serviceType)
        {
            return serviceType
                .GetInterfaces()
                .Concat(new[] { serviceType })
                .SelectMany(x => x.GetMethods())
                .Where(x =>
                {
                    var methodInfo = x;
                    if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) return false;
                    if (methodInfo.GetCustomAttribute<IgnoreAttribute>(false) != null) return false; // ignore

                    var methodName = methodInfo.Name;
                    if (methodName == "Equals"
                        || methodName == "GetHashCode"
                        || methodName == "GetType"
                        || methodName == "ToString"
                        || methodName == "WithOptions"
                        || methodName == "WithHeaders"
                        || methodName == "WithDeadline"
                        || methodName == "WithCancellationToken"
                        || methodName == "WithHost"
                       )
                    {
                        return false;
                    }
                    return true;
                })
                .Where(x => !x.IsSpecialName)
                .Select(x => MagicOnionServiceMethodInfo.Create(serviceType, x))
                .ToArray();
        }

        /// <summary>
        /// Gets the type of wrapped request parameters from the method parameters.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static Type GetRequestTypeFromMethod(MethodInfo methodInfo)
        {
            var parameterTypes = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
            switch (parameterTypes.Length)
            {
                case 0: return typeof(Nil);
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
                default: throw new InvalidOperationException($"A method '{methodInfo.Name}' has too many parameters.");
            }
        }
    }
}
