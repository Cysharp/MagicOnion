using System;
using System.Buffers;
using System.Linq;
using System.Reflection;
using Grpc.Core;
using MessagePack;
using MessagePack.Formatters;

namespace MagicOnion.Serialization.MessagePack
{
    /// <summary>
    /// Provides a <see cref="IMagicOnionSerializerProvider"/> using MessagePack.
    /// </summary>
    public class MessagePackMagicOnionSerializerProvider : IMagicOnionSerializerProvider
    {
        /// <summary>
        /// Gets the provider with <see cref="MessagePackSerializer.DefaultOptions"/> for serialization.
        /// </summary>
        public static MessagePackMagicOnionSerializerProvider Default { get; } = new MessagePackMagicOnionSerializerProvider(MessagePackSerializer.DefaultOptions, enableFallback: false);

        protected MessagePackSerializerOptions SerializerOptions { get; }
        protected bool EnableFallback { get; }

        protected MessagePackMagicOnionSerializerProvider(MessagePackSerializerOptions serializerOptions, bool enableFallback)
        {
            SerializerOptions = serializerOptions;
            EnableFallback = enableFallback;
        }

        public MessagePackMagicOnionSerializerProvider WithOptions(MessagePackSerializerOptions serializerOptions)
        {
            return new MessagePackMagicOnionSerializerProvider(serializerOptions, EnableFallback);
        }

        /// <summary>
        /// Gets a provider with method parameter fallback option. If the option is enabled, a serializer will respects the method's optional parameters.
        /// </summary>
        /// <param name="enableFallback"></param>
        /// <returns></returns>
        public MessagePackMagicOnionSerializerProvider WithEnableFallback(bool enableFallback)
        {
            return new MessagePackMagicOnionSerializerProvider(SerializerOptions, enableFallback);
        }

        public IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo)
        {
            var serializerOptions = EnableFallback && methodInfo != null ? WrapFallbackResolverIfNeeded(methodInfo.GetParameters()) : SerializerOptions;
            return new MessagePackMagicOnionSerializer(serializerOptions);
        }

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

            var formatter = Activator.CreateInstance(formatterType, defaultValues)!;

            return SerializerOptions.WithResolver(new PriorityResolver(t, formatter, SerializerOptions.Resolver));
        }

        class MessagePackMagicOnionSerializer : IMagicOnionSerializer
        {
            readonly MessagePackSerializerOptions serializerOptions;
            public MessagePackMagicOnionSerializer(MessagePackSerializerOptions serializerOptions)
            {
                this.serializerOptions = serializerOptions;
            }

            public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
                => MessagePackSerializer.Deserialize<T>(bytes, serializerOptions);
            public void Serialize<T>(IBufferWriter<byte> writer, in T? value)
                => MessagePackSerializer.Serialize(writer, value, serializerOptions);
        }

        class PriorityResolver : IFormatterResolver
        {
            readonly Type formatterType;
            readonly object formatter;
            readonly IFormatterResolver innerResolver;

            public PriorityResolver(Type formatterType, object formatter, IFormatterResolver innerResolver)
            {
                this.formatterType = formatterType;
                this.formatter = formatter;
                this.innerResolver = innerResolver;
            }

            public IMessagePackFormatter<T> GetFormatter<T>()
            {
                if (typeof(T) == formatterType)
                {
                    return (IMessagePackFormatter<T>)formatter;
                }
                else if (innerResolver == null)
                {
                    return MessagePackSerializer.DefaultOptions.Resolver.GetFormatterWithVerify<T>();
                }
                else
                {
                    return innerResolver.GetFormatterWithVerify<T>();
                }
            }
        }
    }
}
