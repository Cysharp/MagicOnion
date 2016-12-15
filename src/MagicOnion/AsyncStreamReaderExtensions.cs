using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion
{
    public static class AsyncStreamReaderExtensions
    {
        public static async Task ForEachAsync<T>(this IAsyncStreamReader<T> stream, Action<T> action, CancellationToken cancellation = default(CancellationToken))
        {
            using (stream)
            {
                while (!cancellation.IsCancellationRequested && await stream.MoveNext())
                {
                    action(stream.Current);
                }
            }
        }

        public static async Task ForEachAsync<T>(this IAsyncStreamReader<T> stream, Func<T, Task> asyncAction, CancellationToken cancellation = default(CancellationToken))
        {
            using (stream)
            {
                while (!cancellation.IsCancellationRequested && await stream.MoveNext())
                {
                    await asyncAction(stream.Current);
                }
            }
        }

        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IAsyncStreamReader<T> stream)
        {
            return new EnumerableAsyncStreamReader<T>(stream);
        }

        class EnumerableAsyncStreamReader<T> : IAsyncEnumerable<T>
        {
            readonly IAsyncStreamReader<T> stream;

            public EnumerableAsyncStreamReader(IAsyncStreamReader<T> stream)
            {
                this.stream = stream;
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return this.stream;
            }
        }
    }
}
