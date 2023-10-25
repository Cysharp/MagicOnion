using System;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Client.Internal;

namespace MagicOnion.Client.DynamicClient
{
    internal static class RawMethodInvokerTypes
    {
        static readonly MethodInfo create_RefType_RefType = typeof(RawMethodInvoker).GetMethod(nameof(RawMethodInvoker.Create_RefType_RefType), BindingFlags.Static | BindingFlags.Public)!;
        static readonly MethodInfo create_RefType_ValueType = typeof(RawMethodInvoker).GetMethod(nameof(RawMethodInvoker.Create_RefType_ValueType), BindingFlags.Static | BindingFlags.Public)!;
        static readonly MethodInfo create_ValueType_RefType = typeof(RawMethodInvoker).GetMethod(nameof(RawMethodInvoker.Create_ValueType_RefType), BindingFlags.Static | BindingFlags.Public)!;
        static readonly MethodInfo create_ValueType_ValueType = typeof(RawMethodInvoker).GetMethod(nameof(RawMethodInvoker.Create_ValueType_ValueType), BindingFlags.Static | BindingFlags.Public)!;

        public static MethodInfo GetMethodRawInvokerCreateMethod(Type requestType, Type responseType)
        {
            if (requestType.IsValueType && responseType.IsValueType)
            {
                return create_ValueType_ValueType.MakeGenericMethod(requestType, responseType);
            }
            else if (requestType.IsValueType)
            {
                return create_ValueType_RefType.MakeGenericMethod(requestType, responseType);
            }
            else if (responseType.IsValueType)
            {
                return create_RefType_ValueType.MakeGenericMethod(requestType, responseType);
            }

            return create_RefType_RefType.MakeGenericMethod(requestType, responseType);
        }
    }
}
