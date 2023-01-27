using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MessagePack;

namespace MagicOnion
{
    public static class GrpcMethodHelper
    {
        public sealed class MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse>
        {
            public Method<TRawRequest, TRawResponse> Method { get; }
            public Func<TRequest, TRawRequest> ToRawRequest { get; }
            public Func<TResponse, TRawResponse> ToRawResponse { get; }
            public Func<TRawRequest, TRequest> FromRawRequest { get; }
            public Func<TRawResponse, TResponse> FromRawResponse { get; }

            public MagicOnionMethod(Method<TRawRequest, TRawResponse> method)
            {
                Method = method;
                ToRawRequest = ((typeof(TRawRequest) == typeof(Box<TRequest>)) ? (Func<TRequest, TRawRequest>)(x => (TRawRequest)(object)Box.Create(x)) : x => DangerousDummyNull.GetObjectOrDummyNull((TRawRequest)(object)x));
                ToRawResponse = ((typeof(TRawResponse) == typeof(Box<TResponse>)) ? (Func<TResponse, TRawResponse>)(x => (TRawResponse)(object)Box.Create(x)) : x => DangerousDummyNull.GetObjectOrDummyNull((TRawResponse)(object)x));
                FromRawRequest = ((typeof(TRawRequest) == typeof(Box<TRequest>)) ? (Func<TRawRequest, TRequest>)(x => ((Box<TRequest>)(object)x).Value) : x => DangerousDummyNull.GetObjectOrDefault<TRequest>(x));
                FromRawResponse = ((typeof(TRawResponse) == typeof(Box<TResponse>)) ? (Func<TRawResponse, TResponse>)(x => ((Box<TResponse>)(object)x).Value) : x => DangerousDummyNull.GetObjectOrDefault<TResponse>(x));
            }
        }

        public static MagicOnionMethod<Nil, TResponse, Box<Nil>, TRawResponse> CreateMethod<TResponse, TRawResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializer messageSerializer)
        {
            return CreateMethod<TResponse, TRawResponse>(methodType, serviceName, name, null, messageSerializer);
        }
        public static MagicOnionMethod<Nil, TResponse, Box<Nil>, TRawResponse> CreateMethod<TResponse, TRawResponse>(MethodType methodType, string serviceName, string name, MethodInfo methodInfo, IMagicOnionSerializer messageSerializer)
        {
            // WORKAROUND: Prior to MagicOnion 5.0, the request type for the parameter-less method was byte[].
            //             DynamicClient sends byte[], but GeneratedClient sends Nil, which is incompatible,
            //             so as a special case we do not serialize/deserialize and always convert to a fixed values.
            var isMethodResponseTypeBoxed = typeof(TResponse).IsValueType;

            if (isMethodResponseTypeBoxed)
            {
                return new MagicOnionMethod<Nil, TResponse, Box<Nil>, TRawResponse>(new Method<Box<Nil>, TRawResponse>(
                    methodType,
                    serviceName,
                    name,
                    IgnoreNilMarshaller,
                    (Marshaller<TRawResponse>)(object)CreateBoxedMarshaller<TResponse>(messageSerializer, methodType, methodInfo)
                ));
            }
            else
            {
                return new MagicOnionMethod<Nil, TResponse, Box<Nil>, TRawResponse>(new Method<Box<Nil>, TRawResponse>(
                    methodType,
                    serviceName,
                    name,
                    IgnoreNilMarshaller,
                    (Marshaller<TRawResponse>)(object)CreateMarshaller<TResponse>(messageSerializer, methodType, methodInfo)
                ));
            }
        }

        public static MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse> CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializer messageSerializer)
        {
            return CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(methodType, serviceName, name, null, messageSerializer);
        }
        public static MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse> CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType methodType, string serviceName, string name, MethodInfo methodInfo, IMagicOnionSerializer messageSerializer)
        {
            var isMethodRequestTypeBoxed = typeof(TRequest).IsValueType;
            var isMethodResponseTypeBoxed = typeof(TResponse).IsValueType;

            if (isMethodRequestTypeBoxed && isMethodResponseTypeBoxed)
            {
                return new MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse>(new Method<TRawRequest, TRawResponse>(
                    methodType,
                    serviceName,
                    name,
                    (Marshaller<TRawRequest>)(object)CreateBoxedMarshaller<TRequest>(messageSerializer, methodType, methodInfo),
                    (Marshaller<TRawResponse>)(object)CreateBoxedMarshaller<TResponse>(messageSerializer, methodType, methodInfo)
                ));
            }
            else if (isMethodRequestTypeBoxed)
            {
                return new MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse>(new Method<TRawRequest, TRawResponse>(
                    methodType,
                    serviceName,
                    name,
                    (Marshaller<TRawRequest>)(object)CreateBoxedMarshaller<TRequest>(messageSerializer, methodType, methodInfo),
                    (Marshaller<TRawResponse>)(object)CreateMarshaller<TResponse>(messageSerializer, methodType, methodInfo)
                ));
            }
            else if (isMethodResponseTypeBoxed)
            {
                return new MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse>(new Method<TRawRequest, TRawResponse>(
                    methodType,
                    serviceName,
                    name,
                    (Marshaller<TRawRequest>)(object)CreateMarshaller<TRequest>(messageSerializer, methodType, methodInfo),
                    (Marshaller<TRawResponse>)(object)CreateBoxedMarshaller<TResponse>(messageSerializer, methodType, methodInfo)
                ));
            }
            else
            {
                return new MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse>(new Method<TRawRequest, TRawResponse>(
                    methodType,
                    serviceName,
                    name,
                    (Marshaller<TRawRequest>)(object)CreateMarshaller<TRequest>(messageSerializer, methodType, methodInfo),
                    (Marshaller<TRawResponse>)(object)CreateMarshaller<TResponse>(messageSerializer, methodType, methodInfo)
                ));
            }
        }

        // WORKAROUND: Prior to MagicOnion 5.0, the request type for the parameter-less method was byte[].
        //             DynamicClient sends byte[], but GeneratedClient sends Nil, which is incompatible,
        //             so as a special case we do not serialize/deserialize and always convert to a fixed values.
        public static Marshaller<Box<Nil>> IgnoreNilMarshaller { get; } = new Marshaller<Box<Nil>>(
                serializer: (obj, ctx) =>
                {
                    ReadOnlySpan<byte> unsafeNilBytes = new [] { MessagePackCode.Nil };

                    var writer = ctx.GetBufferWriter();
                    var buffer = writer.GetSpan(unsafeNilBytes.Length); // Write `Nil` as `byte[]` to the buffer.
                    unsafeNilBytes.CopyTo(buffer);
                    writer.Advance(unsafeNilBytes.Length);

                    ctx.Complete();
                },
                deserializer: (ctx) => Box.Create(Nil.Default) /* Box.Create always returns cached Box<Nil> */
            );

        static Marshaller<T> CreateMarshaller<T>(IMagicOnionSerializer messageSerializer, MethodType methodType, MethodInfo methodInfo)
        {
            return new Marshaller<T>(
                serializer: (obj, ctx) =>
                {
                    messageSerializer.Serialize(ctx.GetBufferWriter(), DangerousDummyNull.GetObjectOrDefault<T>(obj));
                    ctx.Complete();
                },
                deserializer: (ctx) => DangerousDummyNull.GetObjectOrDummyNull(messageSerializer.Deserialize<T>(ctx.PayloadAsReadOnlySequence())));
        }

        static Marshaller<Box<T>> CreateBoxedMarshaller<T>(IMagicOnionSerializer messageSerializer, MethodType methodType, MethodInfo methodInfo)
        {
            return new Marshaller<Box<T>>(
                serializer: (obj, ctx) =>
                {
                    messageSerializer.Serialize(ctx.GetBufferWriter(), obj.Value);
                    ctx.Complete();
                },
                deserializer: (ctx) => Box.Create(messageSerializer.Deserialize<T>(ctx.PayloadAsReadOnlySequence()))
            );
        }

    }
}
