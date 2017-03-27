#region Copyright notice and license

// Copyright 2015, Google Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.ObjectModel;
using UniRx;
using Grpc.Core.Logging;
using Grpc.Core.Profiling;
using Grpc.Core.Utils;

namespace Grpc.Core.Internal
{
    /// <summary>
    /// Pool of threads polling on a set of completions queues.
    /// </summary>
    internal class GrpcThreadPool
    {
        static readonly ILogger Logger = GrpcEnvironment.Logger.ForType<GrpcThreadPool>();

        readonly GrpcEnvironment environment;
        readonly object myLock = new object();
#if UNITY_METRO
        readonly List<ReadOnlyReactiveProperty<bool>> threads = new List<ReadOnlyReactiveProperty<bool>>();
#else
        readonly List<System.Threading.Thread> threads = new List<System.Threading.Thread>();
#endif
        readonly int poolSize;
        readonly int completionQueueCount;

        readonly List<BasicProfiler> threadProfilers = new List<BasicProfiler>();  // profilers assigned to threadpool threads

        bool stopRequested;

        ReadOnlyCollection<CompletionQueueSafeHandle> completionQueues;

        /// <summary>
        /// Creates a thread pool threads polling on a set of completions queues.
        /// </summary>
        /// <param name="environment">Environment.</param>
        /// <param name="poolSize">Pool size.</param>
        /// <param name="completionQueueCount">Completion queue count.</param>
        public GrpcThreadPool(GrpcEnvironment environment, int poolSize, int completionQueueCount)
        {
            this.environment = environment;
            this.poolSize = poolSize;
            this.completionQueueCount = completionQueueCount;
            GrpcPreconditions.CheckArgument(poolSize >= completionQueueCount,
                "Thread pool size cannot be smaller than the number of completion queues used.");
        }

        public void Start()
        {
            lock (myLock)
            {
                GrpcPreconditions.CheckState(completionQueues == null, "Already started.");
                completionQueues = CreateCompletionQueueList(environment, completionQueueCount);

                for (int i = 0; i < poolSize; i++)
                {
                    var optionalProfiler = i < threadProfilers.Count ? threadProfilers[i] : null;
                    threads.Add(CreateAndStartThread(i, optionalProfiler));
                }
            }
        }

        public IObservable<Unit> StopAsync()
        {
            lock (myLock)
            {
                GrpcPreconditions.CheckState(!stopRequested, "Stop already requested.");
                stopRequested = true;

                foreach (var cq in completionQueues)
                {
                    cq.Shutdown();
                }
            }

            return Observable.Start(() =>
            {
                // Join causes 
                //foreach (var thread in threads)
                //{
                //    thread.Join();
                //}

                foreach (var cq in completionQueues)
                {
                    cq.Dispose();
                }

                for (int i = 0; i < threadProfilers.Count; i++)
                {
                    threadProfilers[i].Dump(string.Format("grpc_trace_thread_{0}.txt", i));
                }
            }, Scheduler.ThreadPool);
        }

        /// <summary>
        /// Returns true if there is at least one thread pool thread that hasn't
        /// already stopped.
        /// Threads can either stop because all completion queues shut down or
        /// because all foreground threads have already shutdown and process is
        /// going to exit.
        /// </summary>
        internal bool IsAlive
        {
            get
            {
#if UNITY_METRO
                return threads.Any(t => t.Value);
#else
                return threads.Any(t => t.ThreadState != ThreadState.Stopped);
#endif
            }
        }

        internal ReadOnlyCollection<CompletionQueueSafeHandle> CompletionQueues
        {
            get
            {
                return completionQueues;
            }
        }
#if UNITY_METRO
        private ReadOnlyReactiveProperty<bool> CreateAndStartThread(int threadIndex, IProfiler optionalProfiler)
        {
            var cqIndex = threadIndex % completionQueues.Count;
            var cq = completionQueues.ElementAt(cqIndex);
            return Observable.Start(() => RunHandlerLoop(cq), Scheduler.ThreadPool)
                .Select(_ => false)
                .ToReadOnlyReactiveProperty(true);
        }
#else
        private System.Threading.Thread CreateAndStartThread(int threadIndex, IProfiler optionalProfiler)
        {
            var cqIndex = threadIndex % completionQueues.Count;
            var cq = completionQueues.ElementAt(cqIndex);

            var thread = new System.Threading.Thread(new ThreadStart(() => RunHandlerLoop(cq, optionalProfiler)));
            thread.IsBackground = true;
            thread.Name = string.Format("grpc {0} (cq {1})", threadIndex, cqIndex);
            thread.Start();

            return thread;
        }
#endif

        /// <summary>
        /// Body of the polling thread.
        /// </summary>
        private void RunHandlerLoop(CompletionQueueSafeHandle cq, IProfiler optionalProfiler)
        {
            if (optionalProfiler != null)
            {
                Profilers.SetForCurrentThread(optionalProfiler);
            }

            CompletionQueueEvent ev;
            do
            {
                ev = cq.Next();
                if (ev.type == CompletionQueueEvent.CompletionType.OpComplete)
                {
                    bool success = (ev.success != 0);
                    IntPtr tag = ev.tag;
                    try
                    {
                        var callback = cq.CompletionRegistry.Extract(tag);
                        callback(success);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Exception occured while invoking completion delegate");
                    }
                }
            }
            while (ev.type != CompletionQueueEvent.CompletionType.Shutdown && !cq.IsClosed); // modified !cq.IsClosed so avoid UnityEditor freeze.
        }

        private static ReadOnlyCollection<CompletionQueueSafeHandle> CreateCompletionQueueList(GrpcEnvironment environment, int completionQueueCount)
        {
            var list = new List<CompletionQueueSafeHandle>();
            for (int i = 0; i < completionQueueCount; i++)
            {
                var completionRegistry = new CompletionRegistry(environment);
                list.Add(CompletionQueueSafeHandle.Create(completionRegistry));
            }
            return list.AsReadOnly();
        }
    }
}
