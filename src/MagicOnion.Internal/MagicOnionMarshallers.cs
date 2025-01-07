using Grpc.Core;
using MessagePack;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MagicOnion.Internal
{
    // invoke from dynamic methods so must be public
    internal static class MagicOnionMarshallers
    {
        public static Marshaller<StreamingHubPayload> StreamingHubMarshaller { get; } = new(
            serializer: static (payload, context) =>
            {
                context.SetPayloadLength(payload.Length);
                var bufferWriter = context.GetBufferWriter();
                payload.Span.CopyTo(bufferWriter.GetSpan(payload.Length));
                bufferWriter.Advance(payload.Length);
                context.Complete();
                StreamingHubPayloadPool.Shared.Return(payload);
            },
            deserializer: static context =>
            {
                return StreamingHubPayloadPool.Shared.RentOrCreate(context.PayloadAsReadOnlySequence());
            }
        );

        [RequiresUnreferencedCode(nameof(MagicOnionMarshallers) + "." + nameof(CreateRequestType) + " is incompatible with trimming and Native AOT.")]
        public static Type CreateRequestType(ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
            {
                return typeof(Nil);
            }
            else if (parameters.Length == 1)
            {
                var t = parameters[0].ParameterType;
                return t;
            }
            else if (parameters.Length >= 16)
            {
                throw new InvalidOperationException($"The method '{parameters[0].Member.DeclaringType!.FullName}.{parameters[0].Member.Name}' must have less than 16 parameters. (Length: {parameters.Length})");
            }
            else
            {
                // start from T2
                var tupleTypeBase = DynamicArgumentTupleTypesCache.Types[parameters.Length - 2];
                var t = tupleTypeBase.MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
                return t;
            }
        }

        [RequiresUnreferencedCode(nameof(MagicOnionMarshallers) + "." + nameof(InstantiateDynamicArgumentTuple) + " is incompatible with trimming and Native AOT.")]
        public static object InstantiateDynamicArgumentTuple(Type[] typeParameters, object[] arguments)
        {
            // start from T2
            var tupleTypeBase = DynamicArgumentTupleTypesCache.Types[arguments.Length - 2];
            return Activator.CreateInstance(tupleTypeBase.MakeGenericType(typeParameters), arguments)!;
        }

        [RequiresUnreferencedCode(nameof(DynamicArgumentTupleTypesCache) + " is incompatible with trimming and Native AOT.")]
        static class DynamicArgumentTupleTypesCache
        {
            public static readonly Type[] Types = typeof(DynamicArgumentTuple<,>).GetTypeInfo().Assembly
                .GetTypes()
                .Where(x => x.Name.StartsWith("DynamicArgumentTuple") && !x.Name.Contains("Formatter"))
                .OrderBy(x => x.GetGenericArguments().Length)
                .ToArray();
        }
    }

}
