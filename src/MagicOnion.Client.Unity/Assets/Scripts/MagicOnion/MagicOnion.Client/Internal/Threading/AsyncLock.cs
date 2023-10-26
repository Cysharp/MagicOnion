using System;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Client.Internal.Threading
{
    internal class AsyncLock
    {
        readonly SemaphoreSlim semaphore;

        public static readonly ValueTask<LockReleaser> EmptyLock = new ValueTask<LockReleaser>(new LockReleaser(null));

        public AsyncLock()
        {
            this.semaphore = new SemaphoreSlim(1, 1);
        }

        public async ValueTask<LockReleaser> LockAsync()
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            return new LockReleaser(semaphore);
        }

        public struct LockReleaser : IDisposable
        {
            readonly SemaphoreSlim? semaphore;

            public LockReleaser(SemaphoreSlim? semaphore)
            {
                this.semaphore = semaphore;
            }

            public void Dispose()
            {
                semaphore?.Release();
            }
        }
    }
}
