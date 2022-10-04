using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    internal static class UnboxAsyncStreamReader
    {
        public static IAsyncStreamReader<T> Create<T, TRaw>(IAsyncStreamReader<TRaw> rawStreamReader)
            => (typeof(TRaw) == typeof(Box<T>)) ? new UnboxAsyncStreamReader<T>((IAsyncStreamReader<Box<T>>)rawStreamReader) : (IAsyncStreamReader<T>)rawStreamReader;
    }

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
