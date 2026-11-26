using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Serialization.MessagePack
{
    /// <summary>
    /// Provides a <see cref="IMagicOnionSerializerProvider"/> using MessagePack.
    /// </summary>
    public class MessagePackMagicOnionSerializerProvider : IMagicOnionSerializerProvider
    {
        /// <summary>
        /// Gets the provider with <see cref="MessagePackSerializerOptions.Default"/> for serialization, or the AOT options when dynamic code is unavailable.
        /// </summary>
        public static MessagePackMagicOnionSerializerProvider Default { get; } = new MessagePackMagicOnionSerializerProvider(GetDefaultOptions(), enableFallback: false);

#if NET6_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = "The JIT options are used only when dynamic code is supported.")]
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Trimmed applications can pass MessagePackSerializerOptions.DefaultAot explicitly.")]
#endif
        static MessagePackSerializerOptions GetDefaultOptions()
        {
#if !NETSTANDARD2_0
            if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                return MessagePackSerializerOptions.DefaultAot;
            }
#endif
            return MessagePackSerializerOptions.Default;
        }

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


#if NET6_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "ClientFactoryProvider is resolved at runtime.")]
#endif
        public IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo)
        {
            var fallback = EnableFallback && methodInfo != null ? CreateFallbackOptions(methodInfo.GetParameters()) : default;
            return new MessagePackMagicOnionSerializer(SerializerOptions, fallback.Options, fallback.RequestType);
        }

#if NET6_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(nameof(DynamicArgumentTupleTypeCache) + " is incompatible with trimming and Native AOT.")]
#endif
        static class DynamicArgumentTupleTypeCache
        {
            public static readonly Type[] Types = typeof(DynamicArgumentTuple<,>).Assembly.GetTypes()
                .Where(x => x.IsGenericTypeDefinition && x.Name.StartsWith("DynamicArgumentTuple", StringComparison.Ordinal))
                .OrderBy(x => x.GetGenericArguments().Length)
                .ToArray();

            public static readonly Type[] FormatterTypes = typeof(DynamicArgumentTupleFormatter<,,>).GetTypeInfo().Assembly
                .GetTypes()
                .Where(x => x.IsGenericTypeDefinition && x.DeclaringType is null && x.Name.StartsWith("DynamicArgumentTupleFormatter", StringComparison.Ordinal))
                .OrderBy(x => x.GetGenericArguments().Length)
                .ToArray();
        }


#if NET6_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(nameof(DynamicArgumentTupleTypeCache) + " is incompatible with trimming and Native AOT.")]
#endif
        (Type? RequestType, MessagePackSerializerOptions? Options) CreateFallbackOptions(ParameterInfo[] parameters)
        {
#if !NETSTANDARD2_0
            if (!RuntimeFeature.IsDynamicCodeSupported) throw new NotSupportedException("When running with AOT runtime, DynamicArgumentTupleFormatter does not support fallback resolver for optional parameters.");
#endif
            // If the method has no parameter or one parameter, we don't need to create fallback resolver for optional parameters.
            if (parameters.Length < 2)
            {
                return default;
            }

            // start from T2
            var formatterTypeBase = DynamicArgumentTupleTypeCache.FormatterTypes[parameters.Length - 2];

            var parameterTypes = parameters.Select(x => x.ParameterType).ToArray();
            var formatterType = formatterTypeBase.MakeGenericType(parameterTypes);
            var requestType = DynamicArgumentTupleTypeCache.Types[parameters.Length - 2].MakeGenericType(parameterTypes);

            var constructorArguments = parameters
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
                }).Prepend(SerializerOptions.Resolver).ToArray();

            var factory = (MessagePackFormatterFactory)Activator.CreateInstance(formatterType,
                BindingFlags.Instance | BindingFlags.NonPublic, binder: null, constructorArguments, culture: null)!;

            return (requestType, SerializerOptions with
            {
                Resolver = new MessagePackFormatterResolver(factory)
                {
                    ThrowOnLegacyFormatter = SerializerOptions.Resolver.ThrowOnLegacyFormatter,
                },
            });
        }

        class MessagePackMagicOnionSerializer(
            MessagePackSerializerOptions serializerOptions,
            MessagePackSerializerOptions? fallbackOptions,
            Type? fallbackRequestType) : IMagicOnionSerializer
        {
            public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
                => MessagePackSerializer.Deserialize<T>(bytes, typeof(T) == fallbackRequestType ? fallbackOptions! : serializerOptions);

            public void Serialize<T>(IBufferWriter<byte> writer, in T? value)
                => MessagePackSerializer.Serialize(writer, value, serializerOptions);
        }

    }
}
