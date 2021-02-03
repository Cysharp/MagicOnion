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
    public class MarshallingAsyncStreamReader<T> : IAsyncStreamReader<T>, IDisposable
    {
        readonly IAsyncStreamReader<byte[]> inner;
        readonly MessagePackSerializerOptions options;

        public MarshallingAsyncStreamReader(IAsyncStreamReader<byte[]> inner, MessagePackSerializerOptions options)
        {
            this.inner = inner;
            this.options = options;
        }

        public T Current { get; private set; }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            if (await inner.MoveNext(cancellationToken))
            {
                this.Current = MessagePackSerializer.Deserialize<T>(inner.Current, options);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            (inner as IDisposable)?.Dispose();
        }
    }

    public class MarshallingClientStreamWriter<T> : IClientStreamWriter<T>
    {
        readonly IClientStreamWriter<byte[]> inner;
        readonly MessagePackSerializerOptions options;

        public MarshallingClientStreamWriter(IClientStreamWriter<byte[]> inner, MessagePackSerializerOptions options)
        {
            this.inner = inner;
            this.options = options;
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

        public Task CompleteAsync()
        {
            return inner.CompleteAsync();
        }

        public Task WriteAsync(T message)
        {
            var bytes = MessagePackSerializer.Serialize(message, options);
            return inner.WriteAsync(bytes);
        }
    }
}