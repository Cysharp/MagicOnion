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
            else if (isMethodRequestTypeBoxed && !isMethodResponseTypeBoxed)
            {
                return new Method<Box<TRequest>, TResponse>(
                    methodType,
                    serviceName,
                    name,
                    CreateBoxedMarshaller<TRequest>(serializerOptions),
                    CreateMarshaller<TResponse>(serializerOptions)
                );
            }
            else if (!isMethodRequestTypeBoxed && isMethodResponseTypeBoxed)
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
