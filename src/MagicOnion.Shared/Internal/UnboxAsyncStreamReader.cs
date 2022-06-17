using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    internal class UnboxAsyncStreamReader<T> : IAsyncStreamReader<T>
    {
        readonly IAsyncStreamReader<Box<T>> inner;

        public UnboxAsyncStreamReader(IAsyncStreamReader<Box<T>> inner)
        {
            this.inner = inner;
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
            => inner.MoveNext(cancellationToken);

        public T Current
            => inner.Current.Value;
    }
}