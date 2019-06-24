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

    public interface IGameLoopLogger
    {
        void Elapsed(int threadNumber, int processCount, int elapsedMilliseconds);
        void UnhandledException(int threadNumber, Exception exception);
    }

    class NullGameLoopLogger : IGameLoopLogger
    {
        public void Elapsed(int threadNumber, int processCount, int elapsedMilliseconds)
        {
        }

        public void UnhandledException(int threadNumber, Exception exception)
        {
        }
    }

    public sealed class GameLoopThreadPool
    {
        readonly GameLoopThread[] threads;
        int index = -1;

        public GameLoopThreadPool(int targetFramerate, int threadPoolCount = -1, IGameLoopLogger logger = null)
        {
            if (threadPoolCount < 0) threadPoolCount = Environment.ProcessorCount;
            threadPoolCount = Math.Max(1, threadPoolCount);

            var pool = new GameLoopThread[threadPoolCount];
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = new GameLoopThread(targetFramerate, i, logger);
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
        readonly IGameLoopLogger logger;

        readonly Stopwatch stopwatch = new Stopwatch();
        bool isStopped = false;

        int tail = 0;
        bool running = false;
        IGameLoopAction[] loopItems = new IGameLoopAction[InitialSize];
        MinimumQueue<IGameLoopAction> waitQueue = new MinimumQueue<IGameLoopAction>(InitialSize);

        public GameLoopThread(int targetFrameRate, int threadNumber, IGameLoopLogger logger)
        {
            this.logger = logger ?? new NullGameLoopLogger();
            this.frameMilliseconds = 1000 / targetFrameRate;
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
                stopwatch.Restart();

                lock (runningAndQueueLock)
                {
                    running = true;
                }

                var processCount = 0;
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
                                processCount++;
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
                                    logger.UnhandledException(threadNumber, ex);
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
                                    processCount++;
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
                                        logger.UnhandledException(threadNumber, ex);
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

                stopwatch.Stop();
                var elapsed = (int)stopwatch.ElapsedMilliseconds;
                logger.Elapsed(threadNumber, processCount, elapsed);

                var restRate = frameMilliseconds - elapsed;
                if (restRate > 0)
                {
                    Thread.Sleep(restRate);
                }
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
