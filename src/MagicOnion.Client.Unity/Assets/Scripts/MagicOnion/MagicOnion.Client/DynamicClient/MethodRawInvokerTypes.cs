using System;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Client.Internal;

namespace MagicOnion.Client.DynamicClient
{
    internal static class MethodRawInvokerTypes
    {
        static readonly MethodInfo unary_Create_RefType_RefType = typeof(UnaryMethodRawInvoker).GetMethod(nameof(UnaryMethodRawInvoker.Create_RefType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo unary_Create_RefType_ValueType = typeof(UnaryMethodRawInvoker).GetMethod(nameof(UnaryMethodRawInvoker.Create_RefType_ValueType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo unary_Create_ValueType_RefType = typeof(UnaryMethodRawInvoker).GetMethod(nameof(UnaryMethodRawInvoker.Create_ValueType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo unary_Create_ValueType_ValueType = typeof(UnaryMethodRawInvoker).GetMethod(nameof(UnaryMethodRawInvoker.Create_ValueType_ValueType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo serverStreaming_Create_RefType_RefType = typeof(ServerStreamingMethodRawInvoker).GetMethod(nameof(ServerStreamingMethodRawInvoker.Create_RefType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo serverStreaming_Create_RefType_ValueType = typeof(ServerStreamingMethodRawInvoker).GetMethod(nameof(ServerStreamingMethodRawInvoker.Create_RefType_ValueType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo serverStreaming_Create_ValueType_RefType = typeof(ServerStreamingMethodRawInvoker).GetMethod(nameof(ServerStreamingMethodRawInvoker.Create_ValueType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo serverStreaming_Create_ValueType_ValueType = typeof(ServerStreamingMethodRawInvoker).GetMethod(nameof(ServerStreamingMethodRawInvoker.Create_ValueType_ValueType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo clientStreaming_Create_RefType_RefType = typeof(ClientStreamingMethodRawInvoker).GetMethod(nameof(ClientStreamingMethodRawInvoker.Create_RefType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo clientStreaming_Create_RefType_ValueType = typeof(ClientStreamingMethodRawInvoker).GetMethod(nameof(ClientStreamingMethodRawInvoker.Create_RefType_ValueType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo clientStreaming_Create_ValueType_RefType = typeof(ClientStreamingMethodRawInvoker).GetMethod(nameof(ClientStreamingMethodRawInvoker.Create_ValueType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo clientStreaming_Create_ValueType_ValueType = typeof(ClientStreamingMethodRawInvoker).GetMethod(nameof(ClientStreamingMethodRawInvoker.Create_ValueType_ValueType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo duplexStreaming_Create_RefType_RefType = typeof(DuplexStreamingMethodRawInvoker).GetMethod(nameof(DuplexStreamingMethodRawInvoker.Create_RefType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo duplexStreaming_Create_RefType_ValueType = typeof(DuplexStreamingMethodRawInvoker).GetMethod(nameof(DuplexStreamingMethodRawInvoker.Create_RefType_ValueType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo duplexStreaming_Create_ValueType_RefType = typeof(DuplexStreamingMethodRawInvoker).GetMethod(nameof(DuplexStreamingMethodRawInvoker.Create_ValueType_RefType), BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo duplexStreaming_Create_ValueType_ValueType = typeof(DuplexStreamingMethodRawInvoker).GetMethod(nameof(DuplexStreamingMethodRawInvoker.Create_ValueType_ValueType), BindingFlags.Static | BindingFlags.Public);

        public static Type GetMethodRawInvokerType(MethodType methodType, Type requestType, Type responseType)
        {
            switch (methodType)
            {
                case MethodType.Unary:
                    return typeof(UnaryMethodRawInvoker<,>).MakeGenericType(requestType, responseType);
                case MethodType.ServerStreaming:
                    return typeof(ServerStreamingMethodRawInvoker<,>).MakeGenericType(requestType, responseType);
                case MethodType.ClientStreaming:
                    return typeof(ClientStreamingMethodRawInvoker<,>).MakeGenericType(requestType, responseType);
                case MethodType.DuplexStreaming:
                    return typeof(DuplexStreamingMethodRawInvoker<,>).MakeGenericType(requestType, responseType);
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
            else if (methodType == MethodType.ServerStreaming)
            {
                return GetServerStreamingMethodRawInvokerCreateMethod(requestType, responseType);
            }
            else if (methodType == MethodType.ClientStreaming)
            {
                return GetClientStreamingMethodRawInvokerCreateMethod(requestType, responseType);
            }
            else if (methodType == MethodType.DuplexStreaming)
            {
                return GetDuplexStreamingMethodRawInvokerCreateMethod(requestType, responseType);
            }

            throw new NotSupportedException();
        }

        private static MethodInfo GetUnaryMethodRawInvokerCreateMethod(Type requestType, Type responseType)
        {
            if (requestType.IsValueType && responseType.IsValueType)
            {
                return unary_Create_ValueType_ValueType.MakeGenericMethod(requestType, responseType);
            }
            else if (requestType.IsValueType)
            {
                return unary_Create_ValueType_RefType.MakeGenericMethod(requestType, responseType);
            }
            else if (responseType.IsValueType)
            {
                return unary_Create_RefType_ValueType.MakeGenericMethod(requestType, responseType);
            }

            return unary_Create_RefType_RefType.MakeGenericMethod(requestType, responseType);
        }

        private static MethodInfo GetServerStreamingMethodRawInvokerCreateMethod(Type requestType, Type responseType)
        {
            if (requestType.IsValueType && responseType.IsValueType)
            {
                return serverStreaming_Create_ValueType_ValueType.MakeGenericMethod(requestType, responseType);
            }
            else if (requestType.IsValueType)
            {
                return serverStreaming_Create_ValueType_RefType.MakeGenericMethod(requestType, responseType);
            }
            else if (responseType.IsValueType)
            {
                return serverStreaming_Create_RefType_ValueType.MakeGenericMethod(requestType, responseType);
            }

            return serverStreaming_Create_RefType_RefType.MakeGenericMethod(requestType, responseType);
        }

        private static MethodInfo GetClientStreamingMethodRawInvokerCreateMethod(Type requestType, Type responseType)
        {
            if (requestType.IsValueType && responseType.IsValueType)
            {
                return clientStreaming_Create_ValueType_ValueType.MakeGenericMethod(requestType, responseType);
            }
            else if (requestType.IsValueType)
            {
                return clientStreaming_Create_ValueType_RefType.MakeGenericMethod(requestType, responseType);
            }
            else if (responseType.IsValueType)
            {
                return clientStreaming_Create_RefType_ValueType.MakeGenericMethod(requestType, responseType);
            }

            return clientStreaming_Create_RefType_RefType.MakeGenericMethod(requestType, responseType);
        }
        
        private static MethodInfo GetDuplexStreamingMethodRawInvokerCreateMethod(Type requestType, Type responseType)
        {
            if (requestType.IsValueType && responseType.IsValueType)
            {
                return duplexStreaming_Create_ValueType_ValueType.MakeGenericMethod(requestType, responseType);
            }
            else if (requestType.IsValueType)
            {
                return duplexStreaming_Create_ValueType_RefType.MakeGenericMethod(requestType, responseType);
            }
            else if (responseType.IsValueType)
            {
                return duplexStreaming_Create_RefType_ValueType.MakeGenericMethod(requestType, responseType);
            }

            return duplexStreaming_Create_RefType_RefType.MakeGenericMethod(requestType, responseType);
        }
    }
}