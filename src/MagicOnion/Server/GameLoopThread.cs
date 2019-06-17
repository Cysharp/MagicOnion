using MagicOnion.Utils;
using System;
using System.Diagnostics;
using System.Threading;

namespace MagicOnion.Server
{
    public interface IGameLoopAction
    {
        bool MoveNext();
    }

    public sealed class GameLoopThreadPool
    {
        readonly GameLoopThread[] threads;
        int index = -1;

        public GameLoopThreadPool(int frameMilliseconds)
        {
            int threadPoolCount = Math.Max(1, Environment.ProcessorCount);

            var pool = new GameLoopThread[threadPoolCount];
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = new GameLoopThread(frameMilliseconds, i);
            }

            this.threads = pool;
        }

        public GameLoopThread GetLoopThread()
        {
            return threads[Interlocked.Increment(ref index) % threads.Length];
        }
    }

    public sealed class GameLoopThread
    {
        const int InitialSize = 16;

        readonly int frameMilliseconds;
        readonly Thread thread;
        readonly int threadNumber = 0;

        readonly object runningAndQueueLock = new object();
        readonly object arrayLock = new object();
        readonly Action<Exception> unhandledExceptionCallback;

        readonly Stopwatch stopwatch = new Stopwatch();
        bool isStopped = false;

        int tail = 0;
        bool running = false;
        IGameLoopAction[] loopItems = new IGameLoopAction[InitialSize];
        MinimumQueue<IGameLoopAction> waitQueue = new MinimumQueue<IGameLoopAction>(InitialSize);

        public GameLoopThread(int frameMilliseconds, int threadNumber)
        {
            // TODO:...
            // this.unhandledExceptionCallback = ex => Debug.LogException(ex);

            this.frameMilliseconds = frameMilliseconds;
            this.thread = new Thread(new ParameterizedThreadStart(RunInThread), 32 * 1024) // Stack, 32K
            {
                Name = "GameLoopThread" + threadNumber,
                Priority = ThreadPriority.Normal,
                IsBackground = true
            };
            this.thread.Start(this);
        }

        public void RegisterAction(IGameLoopAction item)
        {
            lock (runningAndQueueLock)
            {
                if (running)
                {
                    waitQueue.Enqueue(item);
                    return;
                }
            }

            lock (arrayLock)
            {
                // Ensure Capacity
                if (loopItems.Length == tail)
                {
                    Array.Resize(ref loopItems, checked(tail * 2));
                }
                loopItems[tail++] = item;
            }
        }

        static void RunInThread(object objectSelf)
        {
            var self = (GameLoopThread)objectSelf;
            self.Run();
        }

        void Run()
        {
            while (!isStopped)
            {
                lock (runningAndQueueLock)
                {
                    running = true;
                }

                lock (arrayLock)
                {
                    var j = tail - 1;

                    // eliminate array-bound check for i
                    for (int i = 0; i < loopItems.Length; i++)
                    {
                        var action = loopItems[i];
                        if (action != null)
                        {
                            try
                            {
                                if (!action.MoveNext())
                                {
                                    loopItems[i] = null;
                                }
                                else
                                {
                                    continue; // next i 
                                }
                            }
                            catch (Exception ex)
                            {
                                loopItems[i] = null;
                                try
                                {
                                    unhandledExceptionCallback(ex);
                                }
                                catch { }
                            }
                        }

                        // find null, loop from tail
                        while (i < j)
                        {
                            var fromTail = loopItems[j];
                            if (fromTail != null)
                            {
                                try
                                {
                                    if (!fromTail.MoveNext())
                                    {
                                        loopItems[j] = null;
                                        j--;
                                        continue; // next j
                                    }
                                    else
                                    {
                                        // swap
                                        loopItems[i] = fromTail;
                                        loopItems[j] = null;
                                        j--;
                                        goto NEXT_LOOP; // next i
                                    }
                                }
                                catch (Exception ex)
                                {
                                    loopItems[j] = null;
                                    j--;
                                    try
                                    {
                                        unhandledExceptionCallback(ex);
                                    }
                                    catch { }
                                    continue; // next j
                                }
                            }
                            else
                            {
                                j--;
                            }
                        }

                        tail = i; // loop end
                        break; // LOOP END

                        NEXT_LOOP:
                        continue;
                    }


                    lock (runningAndQueueLock)
                    {
                        running = false;
                        while (waitQueue.Count != 0)
                        {
                            if (loopItems.Length == tail)
                            {
                                Array.Resize(ref loopItems, checked(tail * 2));
                            }
                            loopItems[tail++] = waitQueue.Dequeue();
                        }
                    }
                }

                // TODO:sleep logic.
                Thread.Sleep(frameMilliseconds);
            }
        }

        /*
        public void StopThread()
        {
            isStopped = true;
        }
        */
    }
}
