using System;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public class AsyncLock
    {
        readonly SemaphoreSlim semaphore;

        public AsyncLock()
        {
            this.semaphore = new SemaphoreSlim(1, 1);
        }

        public async ValueTask<LockReleaser> LockAsync()
        {
            await semaphore.WaitAsync();
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
                semaphore.Release();
            }
        }
    }
}
