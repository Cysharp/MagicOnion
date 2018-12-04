#if NON_UNITY

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace MagicOnion.Utils
{
    public class ReservedWhenAllPromise : IValueTaskSource
    {
        static class ContinuationSentinel
        {
            public static readonly Action<object> AvailableContinuation = _ => { };
            public static readonly Action<object> CompletedContinuation = _ => { };
        }

        static readonly ContextCallback execContextCallback = ExecutionContextCallback;
        static readonly SendOrPostCallback syncContextCallback = SynchronizationContextCallback;

        int completedCount;
        readonly int resultCount;

        ExceptionDispatchInfo exception;
        Action<object> continuation = ContinuationSentinel.AvailableContinuation;
        object state;
        SynchronizationContext syncContext;
        ExecutionContext execContext;

        public ReservedWhenAllPromise(int reserveCount)
        {
            this.resultCount = reserveCount;
        }

        public ValueTask AsValueTask() => new ValueTask(this, 0);

        public void Add(ValueTask task)
        {
            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                try
                {
                    awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                    return;
                }
                TryInvokeContinuationWithIncrement();
            }
            else
            {
                RegisterUnsafeOnCompleted(awaiter);
            }
        }

        void RegisterUnsafeOnCompleted(ValueTaskAwaiter awaiter)
        {
            awaiter.UnsafeOnCompleted(() => ContinuationWithCapture(awaiter));
        }

        void ContinuationWithCapture(ValueTaskAwaiter awaiter)
        {
            try
            {
                awaiter.GetResult();
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
                TryInvokeContinuation();
                return;
            }
            TryInvokeContinuationWithIncrement();
        }

        void TryInvokeContinuationWithIncrement()
        {
            if (Interlocked.Increment(ref completedCount) == resultCount)
            {
                TryInvokeContinuation();
            }
        }

        void TryInvokeContinuation()
        {
            var c = Interlocked.Exchange(ref continuation, ContinuationSentinel.CompletedContinuation);
            if (c != ContinuationSentinel.AvailableContinuation && c != ContinuationSentinel.CompletedContinuation)
            {
                var spinWait = new SpinWait();
                while (state == null) // worst case, state is not set yet so wait.
                {
                    spinWait.SpinOnce();
                }

                if (execContext != null)
                {
                    ExecutionContext.Run(execContext, execContextCallback, Tuple.Create(c, this));
                }
                else if (syncContext != null)
                {
                    syncContext.Post(syncContextCallback, Tuple.Create(c, this));
                }
                else
                {
                    c(state);
                }
            }
        }

        public void GetResult(short token)
        {
            if (exception != null)
            {
                exception.Throw();
            }
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return (completedCount == resultCount) ? ValueTaskSourceStatus.Succeeded
                : (exception != null) ? ((exception.SourceException is OperationCanceledException) ? ValueTaskSourceStatus.Canceled : ValueTaskSourceStatus.Faulted)
                : ValueTaskSourceStatus.Pending;
        }

        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            if (Interlocked.CompareExchange(ref this.continuation, continuation, ContinuationSentinel.AvailableContinuation) != ContinuationSentinel.AvailableContinuation)
            {
                if (this.continuation == ContinuationSentinel.CompletedContinuation)
                {
                    continuation(state);
                    return;
                }
                throw new InvalidOperationException("does not allow multiple await.");
            }

            this.state = state;
            if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) == ValueTaskSourceOnCompletedFlags.FlowExecutionContext)
            {
                execContext = ExecutionContext.Capture();
            }
            if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) == ValueTaskSourceOnCompletedFlags.UseSchedulingContext)
            {
                syncContext = SynchronizationContext.Current;
            }

            if (GetStatus(token) != ValueTaskSourceStatus.Pending)
            {
                TryInvokeContinuation();
            }
        }

        static void ExecutionContextCallback(object state)
        {
            var t = (Tuple<Action<object>, ReservedWhenAllPromise>)state;
            var self = t.Item2;
            if (self.syncContext != null)
            {
                SynchronizationContextCallback(state);
            }
            else
            {
                var invokeState = self.state;
                self.state = null;
                t.Item1.Invoke(invokeState);
            }
        }

        static void SynchronizationContextCallback(object state)
        {
            var t = (Tuple<Action<object>, ReservedWhenAllPromise>)state;
            var self = t.Item2;
            var invokeState = self.state;
            self.state = null;
            t.Item1.Invoke(invokeState);
        }
    }
}

#endif