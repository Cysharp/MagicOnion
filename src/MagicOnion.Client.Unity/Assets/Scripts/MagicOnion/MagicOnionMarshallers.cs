using Grpc.Core;
using UniRx;
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

        public IObservable<bool> MoveNext()
        {
            return inner.MoveNext().Do(x =>
            {
                if (x)
                {
                    this.Current = marshaller.Deserializer(inner.Current);
                }
            });
        }

        public void Dispose()
        {
            inner.Dispose();
        }
    }

    internal class MarshallingClientStreamWriter<T> : IClientStreamWriter<T>
    {
        readonly IClientStreamWriter<byte[]> inner;
        readonly Marshaller<T> marshaller;

        public MarshallingClientStreamWriter(IClientStreamWriter<byte[]> inner, Marshaller<T> marshaller)
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

        public IObservable<Unit> CompleteAsync()
        {
            return inner.CompleteAsync();
        }

        public IObservable<Unit> WriteAsync(T message)
        {
            var bytes = marshaller.Serializer(message);
            return inner.WriteAsync(bytes);
        }
    }

    public static class MagicOnionMarshallers
    {
        static readonly DirtyTracker NullTracker = new DirtyTracker();
        public static readonly Marshaller<byte[]> ByteArrayMarshaller = Marshallers.Create<byte[]>(x => x, x => x);
        public static readonly byte[] EmptyBytes = new byte[0];

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
    }
}