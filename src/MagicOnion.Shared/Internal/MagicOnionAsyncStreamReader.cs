using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    internal class MagicOnionAsyncStreamReader<T, TRaw> : IAsyncStreamReader<T>
    {
        readonly IAsyncStreamReader<TRaw> inner;
        readonly Func<TRaw, T> fromRawMessage;

        public MagicOnionAsyncStreamReader(IAsyncStreamReader<TRaw> inner, Func<TRaw, T> fromRawMessage)
        {
            this.inner = inner;
            this.fromRawMessage = fromRawMessage;
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
            => inner.MoveNext(cancellationToken);

        public T Current
            => fromRawMessage(inner.Current);
    }
}
