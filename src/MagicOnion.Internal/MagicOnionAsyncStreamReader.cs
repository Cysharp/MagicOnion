using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    internal class MagicOnionAsyncStreamReader<T, TRaw> : IAsyncStreamReader<T>
    {
        readonly IAsyncStreamReader<TRaw> inner;

        public MagicOnionAsyncStreamReader(IAsyncStreamReader<TRaw> inner)
        {
            this.inner = inner;
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
            => inner.MoveNext(cancellationToken);

        public T Current
            => GrpcMethodHelper.FromRaw<TRaw, T>(inner.Current);
    }
}
