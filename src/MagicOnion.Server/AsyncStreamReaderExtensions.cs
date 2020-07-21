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
            using (stream as IDisposable)
            {
                while (!cancellation.IsCancellationRequested && await stream.MoveNext(cancellation))
                {
                    action(stream.Current);
                }
            }
        }

        public static async Task ForEachAsync<T>(this IAsyncStreamReader<T> stream, Func<T, Task> asyncAction, CancellationToken cancellation = default(CancellationToken))
        {
            using (stream as IDisposable)
            {
                while (!cancellation.IsCancellationRequested && await stream.MoveNext(cancellation))
                {
                    await asyncAction(stream.Current);
                }
            }
        }
    }
}
