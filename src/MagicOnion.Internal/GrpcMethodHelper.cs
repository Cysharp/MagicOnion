using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Grpc.Core;
using MagicOnion.Serialization;
using MessagePack;

namespace MagicOnion.Internal
{
    internal static class GrpcMethodHelper
    {
        public sealed class MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse>
        {
            public Method<TRawRequest, TRawResponse> Method { get; }

            public MagicOnionMethod(Method<TRawRequest, TRawResponse> method)
            {
                Method = method;
            }

            public TRawRequest ToRawRequest(TRequest obj) => ToRaw<TRequest, TRawRequest>(obj);
            public TRawResponse ToRawResponse(TResponse obj) => ToRaw<TResponse, TRawResponse>(obj);
            public TRequest FromRawRequest(TRawRequest obj) => FromRaw<TRawRequest, TRequest>(obj);
            public TResponse FromRawResponse(TRawResponse obj) => FromRaw<TRawResponse, TResponse>(obj);

            static TRaw ToRaw<T, TRaw>(T obj)
            {
                if (typeof(TRaw) == typeof(Box<T>))
                {
                    return (TRaw)(object)Box.Create(obj);
                }
                else
                {
                    return DangerousDummyNull.GetObjectOrDummyNull(Unsafe.As<T, TRaw>(ref obj));
                }
            }

            static T FromRaw<TRaw, T>(TRaw obj)
            {
                if (typeof(TRaw) == typeof(Box<T>))
                {
                    return ((Box<T>)(object)obj!).Value;
                }
                else
                {
                    return DangerousDummyNull.GetObjectOrDefault<T>(obj!);
                }
            }
        }

        public static MagicOnionMethod<Nil, TResponse, Box<Nil>, TRawResponse> CreateMethod<TResponse, TRawResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializer messageSerializer)
        {
            return CreateMethod<TResponse, TRawResponse>(methodType, serviceName, name, null, messageSerializer);
        }
        public static MagicOnionMethod<Nil, TResponse, Box<Nil>, TRawResponse> CreateMethod<TResponse, TRawResponse>(MethodType methodType, string serviceName, string name, MethodInfo? methodInfo, IMagicOnionSerializer messageSerializer)
        {
            // WORKAROUND: Prior to MagicOnion 5.0, the request type for the parameter-less method was byte[].
            //             DynamicClient sends byte[], but GeneratedClient sends Nil, which is incompatible,
            //             so as a special case we do not serialize/deserialize and always convert to a fixed values.
            var isMethodResponseTypeBoxed = typeof(TResponse).IsValueType;
            var responseMarshaller = isMethodResponseTypeBoxed
                ? CreateBoxedMarshaller<TResponse>(messageSerializer)
                : (object)CreateMarshaller<TResponse>(messageSerializer);

            return new MagicOnionMethod<Nil, TResponse, Box<Nil>, TRawResponse>(new Method<Box<Nil>, TRawResponse>(
                methodType,
                serviceName,
                name,
                IgnoreNilMarshaller,
                (Marshaller<TRawResponse>)responseMarshaller
            ));
        }

        public static MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse> CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializer messageSerializer)
        {
            return CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(methodType, serviceName, name, null, messageSerializer);
        }
        public static MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse> CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType methodType, string serviceName, string name, MethodInfo? methodInfo, IMagicOnionSerializer messageSerializer)
        {
            var isMethodRequestTypeBoxed = typeof(TRequest).IsValueType;
            var isMethodResponseTypeBoxed = typeof(TResponse).IsValueType;

            var requestMarshaller = isMethodRequestTypeBoxed
                ? CreateBoxedMarshaller<TRequest>(messageSerializer)
                : (object)CreateMarshaller<TRequest>(messageSerializer);
            var responseMarshaller = isMethodResponseTypeBoxed
                ? CreateBoxedMarshaller<TResponse>(messageSerializer)
                : (object)CreateMarshaller<TResponse>(messageSerializer);

            return new MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse>(new Method<TRawRequest, TRawResponse>(
                methodType,
                serviceName,
                name,
                (Marshaller<TRawRequest>)requestMarshaller,
                (Marshaller<TRawResponse>)responseMarshaller
            ));
        }

        // WORKAROUND: Prior to MagicOnion 5.0, the request type for the parameter-less method was byte[].
        //             DynamicClient sends byte[], but GeneratedClient sends Nil, which is incompatible,
        //             so as a special case we do not serialize/deserialize and always convert to a fixed values.
        public static Marshaller<Box<Nil>> IgnoreNilMarshaller { get; } = new Marshaller<Box<Nil>>(
                serializer: (obj, ctx) =>
                {
                    ReadOnlySpan<byte> unsafeNilBytes = new[] { MessagePackCode.Nil };

                    var writer = ctx.GetBufferWriter();
                    var buffer = writer.GetSpan(unsafeNilBytes.Length); // Write `Nil` as `byte[]` to the buffer.
                    unsafeNilBytes.CopyTo(buffer);
                    writer.Advance(unsafeNilBytes.Length);

                    ctx.Complete();
                },
                deserializer: (ctx) => Box.Create(Nil.Default) /* Box.Create always returns cached Box<Nil> */
            );

        static Marshaller<T> CreateMarshaller<T>(IMagicOnionSerializer messageSerializer)
        {
            return new Marshaller<T>(
                serializer: (obj, ctx) =>
                {
#pragma warning disable CS8602
                    if (obj.GetType() == typeof(RawBytesBox))
#pragma warning restore CS8602
                    {
                        var rawBytesBox = (RawBytesBox)(object)obj;
                        var writer = ctx.GetBufferWriter();
                        var buffer = writer.GetSpan(rawBytesBox.Bytes.Length);
                        rawBytesBox.Bytes.Span.CopyTo(buffer);
                        writer.Advance(rawBytesBox.Bytes.Length);
                    }
                    else
                    {
                        messageSerializer.Serialize(ctx.GetBufferWriter(), DangerousDummyNull.GetObjectOrDefault<T>(obj));
                    }
                    ctx.Complete();
                },
                deserializer: (ctx) => DangerousDummyNull.GetObjectOrDummyNull(messageSerializer.Deserialize<T>(ctx.PayloadAsReadOnlySequence()))!);
        }

        static Marshaller<Box<T>> CreateBoxedMarshaller<T>(IMagicOnionSerializer messageSerializer)
        {
            return new Marshaller<Box<T>>(
                serializer: (obj, ctx) =>
                {
#pragma warning disable CS8602
                    if (obj.GetType() == typeof(RawBytesBox))
#pragma warning restore CS8602
                    {
                        var rawBytesBox = (RawBytesBox)(object)obj;
                        var writer = ctx.GetBufferWriter();
                        var buffer = writer.GetSpan(rawBytesBox.Bytes.Length);
                        rawBytesBox.Bytes.Span.CopyTo(buffer);
                        writer.Advance(rawBytesBox.Bytes.Length);
                    }
                    else
                    {
                        messageSerializer.Serialize(ctx.GetBufferWriter(), obj.Value);
                    }
                    ctx.Complete();
                },
                deserializer: (ctx) => Box.Create(messageSerializer.Deserialize<T>(ctx.PayloadAsReadOnlySequence())!)
            );
        }

    }
}
