using Grpc.Core;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion
{
    // invoke from dynamic methods so must be public
    public static class MagicOnionMarshallers
    {
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

        public static readonly byte[] UnsafeNilBytes = new byte[] { MessagePackCode.Nil };

        public static readonly Marshaller<byte[]> ThroughMarshaller = new Marshaller<byte[]>(x => x, x => x);

        internal static Type CreateRequestType(ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
            {
                return typeof(byte[]);
            }
            else if (parameters.Length == 1)
            {
                var t = parameters[0].ParameterType;
                return t;
            }
            else if (parameters.Length >= 16)
            {
                throw new InvalidOperationException($"The method '{parameters[0].Member.DeclaringType.FullName}.{parameters[0].Member.Name}' must have less than 16 parameters. (Length: {parameters.Length})");
            }
            else
            {
                // start from T2
                var tupleTypeBase = dynamicArgumentTupleTypes[parameters.Length - 2];
                var t = tupleTypeBase.MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
                return t;
            }
        }

        public static Type CreateRequestTypeAndSetResolver(string path, ParameterInfo[] parameters, ref IFormatterResolver resolver)
        {
            if (parameters.Length == 0)
            {
                return typeof(byte[]);
            }
            else if (parameters.Length == 1)
            {
                return parameters[0].ParameterType;
            }
            else if (parameters.Length >= 16)
            {
                throw new InvalidOperationException($"The method '{parameters[0].Member.DeclaringType.FullName}.{parameters[0].Member.Name}' must have less than 16 parameters. (Length: {parameters.Length})");
            }
            else
            {
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

                resolver = new PriorityResolver(t, formatter, resolver);
                return t;
            }
        }

        public static object InstantiateDynamicArgumentTuple(Type[] typeParameters, object[] arguments)
        {
            // start from T2
            var tupleTypeBase = dynamicArgumentTupleTypes[arguments.Length - 2];
            return Activator.CreateInstance(tupleTypeBase.MakeGenericType(typeParameters), arguments);
        }
    }

    internal class PriorityResolver : IFormatterResolver
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