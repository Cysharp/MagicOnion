using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Client.DynamicClient
{
    internal class ServiceClientDefinition
    {
        public Type ServiceInterfaceType { get; }
        public IReadOnlyList<ServiceClientMethod> Methods { get; }

        public ServiceClientDefinition(Type serviceInterfaceType, IReadOnlyList<ServiceClientMethod> methods)
        {
            ServiceInterfaceType = serviceInterfaceType;
            Methods = methods;
        }

        public class ServiceClientMethod
        {
            public MethodType MethodType { get; }
            public string ServiceName { get; }
            public string MethodName { get; }
            public string Path { get; }
            public IReadOnlyList<Type> ParameterTypes { get; }
            public Type RequestType { get; }
            public Type ResponseType { get; }
            public ServiceClientMethod(MethodType methodType, string serviceName, string methodName, string path, IReadOnlyList<Type> parameterTypes, Type requestType, Type responseType)
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
                RequestType = requestType;
                ResponseType = responseType;
            }
        }

        public static ServiceClientDefinition CreateFromType<T>()
        {
            return new ServiceClientDefinition(typeof(T), GetServiceMethods(typeof(T)));
        }
        
        private static IReadOnlyList<ServiceClientMethod> GetServiceMethods(Type serviceType)
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
                .Select(x =>
                {
                    var (methodType, responseType) = GetMethodTypeAndResponseTypeFromMethod(x);
                    return new ServiceClientMethod(
                        methodType,
                        serviceType.Name,
                        x.Name,
                        $"{serviceType}/{x.Name}",
                        x.GetParameters().Select(y => y.ParameterType).ToArray(),
                        GetRequestParameterTypeFromMethod(x),
                        responseType
                    );
                })
                .ToArray();
        }

        private static (MethodType MethodType, Type ResponseType) GetMethodTypeAndResponseTypeFromMethod(MethodInfo methodInfo)
        {
            var returnType = methodInfo.ReturnType;
            if (!returnType.IsGenericType)
            {
                throw new InvalidOperationException($"A method '{methodInfo.Name}' returns not supported type.");
            }

            var returnTypeOpen = returnType.GetGenericTypeDefinition();
            if (returnTypeOpen == typeof(UnaryResult<>))
            {
                return (MethodType.Unary, returnType.GetGenericArguments()[0]);
            }
            else if (returnTypeOpen == typeof(ClientStreamingResult<,>))
            {
                return (MethodType.ClientStreaming, returnType.GetGenericArguments()[1]);
            }
            else if (returnTypeOpen == typeof(ServerStreamingResult<>))
            {
                return (MethodType.ServerStreaming, returnType.GetGenericArguments()[0]);
            }
            else if (returnTypeOpen == typeof(DuplexStreamingResult<,>))
            {
                return (MethodType.DuplexStreaming, returnType.GetGenericArguments()[1]);
            }
            else
            {
                throw new InvalidOperationException($"A method '{methodInfo.Name}' returns not supported type.");
            }
        }

        /// <summary>
        /// Gets the type of wrapped request parameters from the method parameters.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static Type GetRequestParameterTypeFromMethod(MethodInfo methodInfo)
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
                case 10: return typeof(DynamicArgumentTuple<,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9]);
                case 11: return typeof(DynamicArgumentTuple<,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10]);
                case 12: return typeof(DynamicArgumentTuple<,,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10], parameterTypes[11]);
                case 13: return typeof(DynamicArgumentTuple<,,,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10], parameterTypes[11], parameterTypes[12]);
                case 14: return typeof(DynamicArgumentTuple<,,,,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10], parameterTypes[11], parameterTypes[12], parameterTypes[13]);
                case 15: return typeof(DynamicArgumentTuple<,,,,,,,,,,,,,>).MakeGenericType(parameterTypes[0], parameterTypes[1], parameterTypes[2], parameterTypes[3], parameterTypes[4], parameterTypes[5], parameterTypes[6], parameterTypes[7], parameterTypes[8], parameterTypes[9], parameterTypes[10], parameterTypes[11], parameterTypes[12], parameterTypes[13], parameterTypes[14]);
                default: throw new InvalidOperationException($"A method '{methodInfo.Name}' has too many parameters.");
            }
        }
    }
}