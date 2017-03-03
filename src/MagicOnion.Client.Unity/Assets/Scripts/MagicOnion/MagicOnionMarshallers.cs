using Grpc.Core;
using MessagePack;
using UniRx;

namespace MagicOnion
{
    internal class MarshallingAsyncStreamReader<T> : IAsyncStreamReader<T>
    {
        readonly IAsyncStreamReader<byte[]> inner;
        readonly IFormatterResolver resolver;

        public MarshallingAsyncStreamReader(IAsyncStreamReader<byte[]> inner, IFormatterResolver resolver)
        {
            this.inner = inner;
            this.resolver = resolver;
        }

        public T Current { get; private set; }

        public IObservable<bool> MoveNext()
        {
            return inner.MoveNext().Do(x =>
            {
                if (x)
                {
                    this.Current = MessagePackSerializer.Deserialize<T>(inner.Current, resolver);
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
        readonly IFormatterResolver resolver;

        public MarshallingClientStreamWriter(IClientStreamWriter<byte[]> inner, IFormatterResolver resolver)
        {
            this.inner = inner;
            this.resolver = resolver;
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
            var bytes = MessagePackSerializer.Serialize(message, resolver);
            return inner.WriteAsync(bytes);
        }
    }

    public static class MagicOnionMarshallers
    {
        public static readonly Marshaller<byte[]> ThroughMarshaller = Marshallers.Create<byte[]>(x => x, x => x);
        public static readonly byte[] UnsafeNilBytes = new byte[] { MessagePackCode.Nil };
    }
}