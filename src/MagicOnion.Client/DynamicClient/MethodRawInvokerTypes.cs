using System;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Client.Internal;

namespace MagicOnion.Client.DynamicClient
{
    internal static class MethodRawInvokerTypes
    {
        static readonly MethodInfo unaryMethodRawInvoker_Create_RefType_RefType = typeof(UnaryMethodRawInvoker).GetMethod(nameof(UnaryMethodRawInvoker.Create_RefType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo unaryMethodRawInvoker_Create_RefType_ValueType = typeof(UnaryMethodRawInvoker).GetMethod(nameof(UnaryMethodRawInvoker.Create_RefType_ValueType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo unaryMethodRawInvoker_Create_ValueType_RefType = typeof(UnaryMethodRawInvoker).GetMethod(nameof(UnaryMethodRawInvoker.Create_ValueType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo unaryMethodRawInvoker_Create_ValueType_ValueType = typeof(UnaryMethodRawInvoker).GetMethod(nameof(UnaryMethodRawInvoker.Create_ValueType_ValueType), BindingFlags.Static | BindingFlags.Public);

        public static Type GetMethodRawInvokerType(MethodType methodType, Type requestType, Type responseType)
        {
            switch (methodType)
            {
                case MethodType.Unary:
                    return typeof(UnaryMethodRawInvoker<,>).MakeGenericType(requestType, responseType);
                    break;
                case MethodType.ServerStreaming:
                    throw new NotSupportedException($"DynamicClientBuilder does not support MethodType '{methodType}'.");
                    break;
                case MethodType.ClientStreaming:
                    throw new NotSupportedException($"DynamicClientBuilder does not support MethodType '{methodType}'.");
                    break;
                case MethodType.DuplexStreaming:
                    throw new NotSupportedException($"DynamicClientBuilder does not support MethodType '{methodType}'.");
                    break;
                default:
                    throw new NotSupportedException($"DynamicClientBuilder does not support MethodType '{methodType}'.");
            }
        }

        public static MethodInfo GetMethodRawInvokerCreateMethod(MethodType methodType, Type requestType, Type responseType)
        {
            if (methodType == MethodType.Unary)
            {
                return GetUnaryMethodRawInvokerCreateMethod(requestType, responseType);
            }

            throw new NotSupportedException();
        }

        public static MethodInfo GetUnaryMethodRawInvokerCreateMethod(Type requestType, Type responseType)
        {
            if (requestType.IsValueType && responseType.IsValueType)
            {
                return unaryMethodRawInvoker_Create_ValueType_ValueType.MakeGenericMethod(requestType, responseType);
            }
            else if (requestType.IsValueType)
            {
                return unaryMethodRawInvoker_Create_ValueType_RefType.MakeGenericMethod(requestType, responseType);
            }
            else if (responseType.IsValueType)
            {
                return unaryMethodRawInvoker_Create_RefType_ValueType.MakeGenericMethod(requestType, responseType);
            }

            return unaryMethodRawInvoker_Create_RefType_RefType.MakeGenericMethod(requestType, responseType);
        }
    }
}