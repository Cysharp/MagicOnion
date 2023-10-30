using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    internal class MagicOnionServerStreamWriter<T, TRaw> : IServerStreamWriter<T>
    {
        readonly IServerStreamWriter<TRaw> inner;
        readonly Func<T, TRaw> toRawMessage;

        public MagicOnionServerStreamWriter(IServerStreamWriter<TRaw> inner, Func<T, TRaw> toRawMessage)
        {
            this.inner = inner;
            this.toRawMessage = toRawMessage;
        }

        public Task WriteAsync(T message)
            => inner.WriteAsync(toRawMessage(message));

        public WriteOptions? WriteOptions
        {
            get => inner.WriteOptions;
            set => inner.WriteOptions = value;
        }
    }
}
