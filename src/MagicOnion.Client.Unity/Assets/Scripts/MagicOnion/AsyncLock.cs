using System;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion
{
    public class AsyncLock
    {
        readonly SemaphoreSlim semaphore;

#if NON_UNITY
        public static readonly ValueTask<LockReleaser> EmptyLock = new ValueTask<LockReleaser>(new LockReleaser(null));
#endif

        public AsyncLock()
        {
            this.semaphore = new SemaphoreSlim(1, 1);
        }

#if NON_UNITY
        public async ValueTask<LockReleaser> LockAsync()
#else
        public async Task<LockReleaser> LockAsync()
#endif
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            return new LockReleaser(semaphore);
        }

        public struct LockReleaser : IDisposable
        {
            readonly SemaphoreSlim semaphore;

            public LockReleaser(SemaphoreSlim semaphore)
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
