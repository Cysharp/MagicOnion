using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using MessagePack;

namespace MagicOnion
{
    public static class GrpcMethodHelper
    {
        public class Box<T>
        {
            public readonly T Value;

            public Box(T value)
            {
                Value = value;
            }
        }

        public static IMethod CreateMethod<TResponse>(MethodType methodType, string serviceName, string name, MessagePackSerializerOptions serializerOptions)
        {
            // WORKAROUND: Prior to MagicOnion 5.0, the request type for the parameter-less method was byte[].
            //             DynamicClient sends byte[], but GeneratedClient sends Nil, which is incompatible,
            //             so as a special case we do not serialize/deserialize and always convert to a fixed values.
            var isMethodResponseTypeBoxed = typeof(TResponse).IsValueType;

            if (isMethodResponseTypeBoxed)
            {
                return new Method<Box<Nil>, Box<TResponse>>(
                    methodType,
                    serviceName,
                    name,
                    IgnoreNilMarshaller,
                    CreateBoxedMarshaller<TResponse>(serializerOptions)
                );
            }
            else
            {
                return new Method<Box<Nil>, TResponse>(
                    methodType,
                    serviceName,
                    name,
                    IgnoreNilMarshaller,
                    CreateMarshaller<TResponse>(serializerOptions)
                );
            }
        }

        public static IMethod CreateMethod<TRequest, TResponse>(MethodType methodType, string serviceName, string name, MessagePackSerializerOptions serializerOptions)
        {
            var isMethodRequestTypeBoxed = typeof(TRequest).IsValueType;
            var isMethodResponseTypeBoxed = typeof(TResponse).IsValueType;

            if (isMethodRequestTypeBoxed && isMethodResponseTypeBoxed)
            {
                return new Method<Box<TRequest>, Box<TResponse>>(
                    methodType,
                    serviceName,
                    name,
                    CreateBoxedMarshaller<TRequest>(serializerOptions),
                    CreateBoxedMarshaller<TResponse>(serializerOptions)
                );
            }
            else if (isMethodRequestTypeBoxed)
            {
                return new Method<Box<TRequest>, TResponse>(
                    methodType,
                    serviceName,
                    name,
                    CreateBoxedMarshaller<TRequest>(serializerOptions),
                    CreateMarshaller<TResponse>(serializerOptions)
                );
            }
            else if (isMethodResponseTypeBoxed)
            {
                return new Method<TRequest, Box<TResponse>>(
                    methodType,
                    serviceName,
                    name,
                    CreateMarshaller<TRequest>(serializerOptions),
                    CreateBoxedMarshaller<TResponse>(serializerOptions)
                );
            }
            else
            {
                return new Method<TRequest, TResponse>(
                    methodType,
                    serviceName,
                    name,
                    CreateMarshaller<TRequest>(serializerOptions),
                    CreateMarshaller<TResponse>(serializerOptions)
                );
            }
        }

        private static readonly Box<Nil> BoxedNil = new Box<Nil>(Nil.Default);

        // WORKAROUND: Prior to MagicOnion 5.0, the request type for the parameter-less method was byte[].
        //             DynamicClient sends byte[], but GeneratedClient sends Nil, which is incompatible,
        //             so as a special case we do not serialize/deserialize and always convert to a fixed values.
        public static Marshaller<Box<Nil>> IgnoreNilMarshaller { get; } = new Marshaller<Box<Nil>>(
                serializer: (obj, ctx) =>
                {
                    var writer = ctx.GetBufferWriter();
                    var buffer = writer.GetSpan(MagicOnionMarshallers.UnsafeNilBytes.Length);
                    MagicOnionMarshallers.UnsafeNilBytes.CopyTo(buffer);
                    writer.Advance(buffer.Length);

                    ctx.Complete();
                },
                deserializer: (ctx) => BoxedNil
            );

        private static Marshaller<T> CreateMarshaller<T>(MessagePackSerializerOptions serializerOptions)
            => new Marshaller<T>(
                serializer: (obj, ctx) =>
                {
                    MessagePackSerializer.Serialize(ctx.GetBufferWriter(), obj, serializerOptions);
                    ctx.Complete();
                },
                deserializer: (ctx) => MessagePackSerializer.Deserialize<T>(ctx.PayloadAsReadOnlySequence(), serializerOptions)
            );

        private static Marshaller<Box<T>> CreateBoxedMarshaller<T>(MessagePackSerializerOptions serializerOptions)
            => new Marshaller<Box<T>>(
                serializer: (obj, ctx) =>
                {
                    MessagePackSerializer.Serialize(ctx.GetBufferWriter(), obj.Value, serializerOptions);
                    ctx.Complete();
                },
                deserializer: (ctx) => new Box<T>(MessagePackSerializer.Deserialize<T>(ctx.PayloadAsReadOnlySequence(), serializerOptions))
            );
    }
}
