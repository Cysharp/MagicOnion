using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Utils;
using MessagePack;

namespace MagicOnion
{
    public interface IMagicOnionMessageSerializer
    {
#if NET5_0_OR_GREATER
        Marshaller<T> CreateSerializer<T>(MethodType methodType, MethodInfo? methodInfo);
#else
        Marshaller<T> CreateSerializer<T>(MethodType methodType, MethodInfo methodInfo);
#endif
        void Serialize<T>(IBufferWriter<byte> writer, in T value);
        T Deserialize<T>(in ReadOnlySequence<byte> bytes);
    }


    public abstract class MagicOnionMessagePackMessageSerializer : IMagicOnionMessageSerializer
    {
        public static MagicOnionMessagePackMessageSerializer Default { get; } = new MagicOnionMessagePackMessageSerializerImpl(MessagePackSerializer.DefaultOptions, enableFallback: false);

        protected MessagePackSerializerOptions SerializerOptions { get; }
        protected bool EnableFallback { get; }

        protected MagicOnionMessagePackMessageSerializer(MessagePackSerializerOptions serializerOptions, bool enableFallback)
        {
            SerializerOptions = serializerOptions;
            EnableFallback = enableFallback;
        }

        public MagicOnionMessagePackMessageSerializer WithOptions(MessagePackSerializerOptions serializerOptions)
        {
            return Create(serializerOptions, EnableFallback);
        }

        public MagicOnionMessagePackMessageSerializer WithEnableFallback(bool enableFallback)
        {
            return Create(SerializerOptions, enableFallback);
        }

#if NET5_0_OR_GREATER
        public Marshaller<T> CreateSerializer<T>(MethodType methodType, MethodInfo? methodInfo)
#else
        public Marshaller<T> CreateSerializer<T>(MethodType methodType, MethodInfo methodInfo)
#endif
        {
            var serializerOptions = (EnableFallback && methodInfo != null) ? WrapFallbackResolverIfNeeded(methodInfo.GetParameters()) : SerializerOptions;
            return Marshallers.Create<T>(
                serializer: (value, ctx) =>
                {
                    Serialize(ctx.GetBufferWriter(), value, serializerOptions);
                    ctx.Complete();
                },
                deserializer: (ctx) => Deserialize<T>(ctx.PayloadAsReadOnlySequence(), serializerOptions));
        }

        public void Serialize<T>(IBufferWriter<byte> writer, in T value)
            => Serialize<T>(writer, value, SerializerOptions);
        public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
            => Deserialize<T>(bytes, SerializerOptions);

        protected abstract MagicOnionMessagePackMessageSerializer Create(MessagePackSerializerOptions serializerOptions, bool enableFallback);
        protected abstract T Deserialize<T>(in ReadOnlySequence<byte> bytes, MessagePackSerializerOptions serializerOptions);
        protected abstract void Serialize<T>(IBufferWriter<byte> writer, in T value, MessagePackSerializerOptions serializerOptions);


        static readonly Type[] dynamicArgumentTupleTypes = typeof(DynamicArgumentTuple<,>).GetTypeInfo().Assembly
            .GetTypes()
            .Where(x => x.Name.StartsWith("DynamicArgumentTuple") && !x.Name.Contains("Formatter"))
            .OrderBy(x => x.GetGenericArguments().Length)
            .ToArray();

        static readonly Type[] dynamicArgumentTupleFormatterTypes = typeof(DynamicArgumentTupleFormatter<,,>).GetTypeInfo().Assembly
            .GetTypes()
            .Where(x => x.Name.StartsWith("DynamicArgumentTupleFormatter"))
            .OrderBy(x => x.GetGenericArguments().Length)
            .ToArray();

        MessagePackSerializerOptions WrapFallbackResolverIfNeeded(ParameterInfo[] parameters)
        {
            // If the method has no parameter or one parameter, we don't need to create fallback resolver for optional parameters.
            if (parameters.Length < 2)
            {
                return SerializerOptions;
            }

            // start from T2
            var tupleTypeBase = dynamicArgumentTupleTypes[parameters.Length - 2];
            var formatterTypeBase = dynamicArgumentTupleFormatterTypes[parameters.Length - 2];

            var t = tupleTypeBase.MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
            var formatterType = formatterTypeBase.MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());

            var defaultValues = parameters
                .Select(x =>
                {
                    if (x.HasDefaultValue)
                    {
                        return x.DefaultValue;
                    }
                    else if (x.ParameterType.GetTypeInfo().IsValueType)
                    {
                        return Activator.CreateInstance(x.ParameterType);
                    }
                    else
                    {
                        return null;
                    }
                }).ToArray();

            var formatter = Activator.CreateInstance(formatterType, defaultValues);

            return SerializerOptions.WithResolver(new PriorityResolver(t, formatter, SerializerOptions.Resolver));
        }


        class MagicOnionMessagePackMessageSerializerImpl : MagicOnionMessagePackMessageSerializer
        {
            public MagicOnionMessagePackMessageSerializerImpl(MessagePackSerializerOptions serializerOptions, bool enableFallback) : base(serializerOptions, enableFallback) {}

            protected override MagicOnionMessagePackMessageSerializer Create(MessagePackSerializerOptions serializerOptions, bool enableFallback)
            {
                return new MagicOnionMessagePackMessageSerializerImpl(serializerOptions, enableFallback);
            }

            protected override T Deserialize<T>(in ReadOnlySequence<byte> bytes, MessagePackSerializerOptions serializerOptions)
            {
                return MessagePackSerializer.Deserialize<T>(bytes, serializerOptions);
            }
            protected override void Serialize<T>(IBufferWriter<byte> writer, in T value, MessagePackSerializerOptions serializerOptions)
            {
                MessagePackSerializer.Serialize(writer, value, serializerOptions);
            }
        }
    }
}
