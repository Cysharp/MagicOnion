using System.Runtime.CompilerServices;
using Grpc.Core;
using MagicOnion.Serialization;
using MessagePack;

namespace MagicOnion.Internal;

internal static class GrpcMethodHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRaw ToRaw<T, TRaw>(T obj)
        => (typeof(T).IsValueType)
            ? (TRaw)(object)Box.Create(obj)
            : DangerousDummyNull.GetObjectOrDummyNull(Unsafe.As<T, TRaw>(ref obj));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T FromRaw<TRaw, T>(TRaw obj)
        => (obj is Box<T> boxed)
            ? boxed.Value
            : DangerousDummyNull.GetObjectOrDefault<T>(obj!);

    public static Method<Box<Nil>, TRawResponse> CreateMethod<TResponse, TRawResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializer messageSerializer)
        where TRawResponse : class
    {
        // WORKAROUND: Prior to MagicOnion 5.0, the request type for the parameter-less method was byte[].
        //             DynamicClient sends byte[], but GeneratedClient sends Nil, which is incompatible,
        //             so as a special case we do not serialize/deserialize and always convert to a fixed values.
        var isMethodResponseTypeBoxed = typeof(TResponse).IsValueType;
        var responseMarshaller = isMethodResponseTypeBoxed
            ? CreateBoxedMarshaller<TResponse>(messageSerializer)
            : (object)CreateMarshaller<TResponse>(messageSerializer);

        return new Method<Box<Nil>, TRawResponse>(
            methodType,
            serviceName,
            name,
            IgnoreNilMarshaller,
            (Marshaller<TRawResponse>)responseMarshaller
        );
    }

    public static Method<TRawRequest, TRawResponse> CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializer messageSerializer)
        where TRawRequest : class
        where TRawResponse : class
    {
        var isMethodRequestTypeBoxed = typeof(TRequest).IsValueType;
        var isMethodResponseTypeBoxed = typeof(TResponse).IsValueType;

        var requestMarshaller = isMethodRequestTypeBoxed
            ? CreateBoxedMarshaller<TRequest>(messageSerializer)
            : (object)CreateMarshaller<TRequest>(messageSerializer);
        var responseMarshaller = isMethodResponseTypeBoxed
            ? CreateBoxedMarshaller<TResponse>(messageSerializer)
            : (object)CreateMarshaller<TResponse>(messageSerializer);

        return new Method<TRawRequest, TRawResponse>(
            methodType,
            serviceName,
            name,
            (Marshaller<TRawRequest>)requestMarshaller,
            (Marshaller<TRawResponse>)responseMarshaller
        );
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
