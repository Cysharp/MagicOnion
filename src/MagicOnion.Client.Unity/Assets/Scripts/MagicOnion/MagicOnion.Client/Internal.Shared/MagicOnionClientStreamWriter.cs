using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    internal class MagicOnionClientStreamWriter<T, TRaw> : IClientStreamWriter<T>
    {
        readonly IClientStreamWriter<TRaw> inner;
        readonly Func<T, TRaw> toRawMessage;

        public MagicOnionClientStreamWriter(IClientStreamWriter<TRaw> inner, Func<T, TRaw> toRawMessage)
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
        public Task CompleteAsync()
            => inner.CompleteAsync();
    }
}
