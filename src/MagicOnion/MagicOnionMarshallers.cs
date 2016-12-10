using Grpc.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ZeroFormatter;
using ZeroFormatter.Formatters;
using ZeroFormatter.Internal;

namespace MagicOnion
{
    internal class MarshallingAsyncStreamReader<TRequest> : IAsyncStreamReader<TRequest>
    {
        readonly IAsyncStreamReader<byte[]> inner;
        readonly Marshaller<TRequest> marshaller;

        public MarshallingAsyncStreamReader(IAsyncStreamReader<byte[]> inner, Marshaller<TRequest> marshaller)
        {
            this.inner = inner;
            this.marshaller = marshaller;
        }

        public TRequest Current { get; private set; }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            if (await inner.MoveNext(cancellationToken))
            {
                this.Current = marshaller.Deserializer(inner.Current);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            inner.Dispose();
        }
    }

    internal class MarshallingServerStreamWriter<TResponse> : IServerStreamWriter<TResponse>
    {
        readonly IServerStreamWriter<byte[]> inner;
        readonly Marshaller<TResponse> marshaller;

        public MarshallingServerStreamWriter(IServerStreamWriter<byte[]> inner, Marshaller<TResponse> marshaller)
        {
            this.inner = inner;
            this.marshaller = marshaller;
        }

        public WriteOptions WriteOptions
        {
            get
            {
                return inner.WriteOptions;
            }

            set
            {
                inner.WriteOptions = value;
            }
        }

        public Task WriteAsync(TResponse message)
        {
            var bytes = marshaller.Serializer(message);
            return inner.WriteAsync(bytes);
        }
    }

    // invoke from dynamic methods so must be public
    public static class MagicOnionMarshallers
    {
        static readonly DirtyTracker NullTracker = new DirtyTracker();

        static readonly Type[] dynamicArgumentTupleTypes = typeof(DynamicArgumentTuple<,>).Assembly
            .GetTypes()
            .Where(x => x.Name.StartsWith("DynamicArgumentTuple") && !x.Name.Contains("Formatter"))
            .OrderBy(x => x.GetGenericArguments().Length)
            .ToArray();

        static readonly Type[] dynamicArgumentTupleFormatterTypes = typeof(DynamicArgumentTupleFormatter<,,>).Assembly
            .GetTypes()
            .Where(x => x.Name.StartsWith("DynamicArgumentTupleFormatter"))
            .OrderBy(x => x.GetGenericArguments().Length)
            .ToArray();

        public static readonly Marshaller<byte[]> ByteArrayMarshaller = Marshallers.Create<byte[]>(x => x, x => x);

        public static Marshaller<T> CreateZeroFormatterMarshaller<TTypeResolver, T>(Formatter<TTypeResolver, T> formatter)
            where TTypeResolver : ITypeResolver, new()
        {
            if (typeof(T) == typeof(byte[]))
            {
                return (Marshaller<T>)(object)ByteArrayMarshaller;
            }

            var noUseDirtyTracker = formatter.NoUseDirtyTracker;

            return new Marshaller<T>(x =>
            {
                byte[] bytes = null;
                var size = formatter.Serialize(ref bytes, 0, x);
                if (bytes.Length != size)
                {
                    BinaryUtil.FastResize(ref bytes, size);
                }
                return bytes;
            }, bytes =>
            {
                var tracker = noUseDirtyTracker ? NullTracker : new DirtyTracker();
                int _;
                return formatter.Deserialize(ref bytes, 0, tracker, out _);
            });
        }

        internal static object CreateZeroFormattertMarshallerReflection(Type resolverType, Type elementType)
        {
            if (elementType == typeof(byte[])) return ByteArrayMarshaller;

            var formatter = typeof(Formatter<,>).MakeGenericType(resolverType, elementType)
                .GetProperty("Default")
                .GetGetMethod()
                .Invoke(null, Type.EmptyTypes);

            return CreateZeroFormattertMarshallerReflection(resolverType, elementType, formatter);
        }

        internal static object CreateZeroFormattertMarshallerReflection(Type resolverType, Type elementType, object formatter)
        {
            if (elementType == typeof(byte[])) return ByteArrayMarshaller;

            var marshaller = typeof(MagicOnionMarshallers).GetMethod("CreateZeroFormatterMarshaller", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .MakeGenericMethod(resolverType, elementType)
                .Invoke(null, new object[] { formatter });

            return marshaller;
        }

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
            else if (parameters.Length > 20)
            {
                throw new InvalidOperationException($"Parameter length must be <= 20, length:{parameters.Length}");
            }
            else
            {
                // start from T2
                var tupleTypeBase = dynamicArgumentTupleTypes[parameters.Length - 2];
                var formatterTypeBase = dynamicArgumentTupleFormatterTypes[parameters.Length - 2];

                var t = tupleTypeBase.MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
                return t;
            }
        }

        public static Type CreateRequestTypeAndMarshaller(Type resolverType, string path, ParameterInfo[] parameters, out object marshaller)
        {
            if (parameters.Length == 0)
            {
                marshaller = MagicOnionMarshallers.ByteArrayMarshaller;
                return typeof(byte[]);
            }
            else if (parameters.Length == 1)
            {
                var t = parameters[0].ParameterType;
                marshaller = MagicOnionMarshallers.CreateZeroFormattertMarshallerReflection(resolverType, t);
                return t;
            }
            else if (parameters.Length > 20)
            {
                throw new InvalidOperationException($"Parameter length must be <= 20, path:{path} length:{parameters.Length}");
            }
            else
            {
                // start from T2
                var tupleTypeBase = dynamicArgumentTupleTypes[parameters.Length - 2];
                var formatterTypeBase = dynamicArgumentTupleFormatterTypes[parameters.Length - 2];

                var t = tupleTypeBase.MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
                var formatterType = formatterTypeBase.MakeGenericType(new[] { resolverType }.Concat(parameters.Select(x => x.ParameterType)).ToArray());

                var defaultValues = parameters
                    .Select(x =>
                    {
                        if (x.HasDefaultValue)
                        {
                            return x.DefaultValue;
                        }
                        else if (x.ParameterType.IsValueType)
                        {
                            return Activator.CreateInstance(x.ParameterType);
                        }
                        else
                        {
                            return null;
                        }
                    }).ToArray();

                var formatter = Activator.CreateInstance(formatterType, defaultValues);

                marshaller = MagicOnionMarshallers.CreateZeroFormattertMarshallerReflection(resolverType, t, formatter);
                return t;
            }
        }
    }
}