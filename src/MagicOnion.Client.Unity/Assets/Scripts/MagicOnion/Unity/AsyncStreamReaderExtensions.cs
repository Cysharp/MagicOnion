using Grpc.Core;
using System;
using System.Threading;
using UniRx;

namespace MagicOnion
{
    public static class AsyncStreamReaderExtensions
    {
        public static IDisposable Subscribe<T>(this IAsyncStreamReader<T> stream, Action<T> onNext, bool observeOnMainThread = true, IDisposable streamingResult = null)
        {
            if (observeOnMainThread)
            {
                return AsObservable(stream, observeOnMainThread, streamingResult).Subscribe(onNext);
            }

            var subscription = ForEachAsync<T>(stream, onNext).Subscribe();

            if (streamingResult == null) return subscription;

            return StableCompositeDisposable.Create(subscription, streamingResult);
        }

        public static IDisposable Subscribe<T>(this IAsyncStreamReader<T> stream, IObserver<T> observer, bool observeOnMainThread = true, IDisposable streamingResult = null)
        {
            var subscription = AsObservable(stream, observeOnMainThread).Subscribe(observer);

            if (streamingResult == null) return subscription;
            return StableCompositeDisposable.Create(subscription, streamingResult);
        }

        public static IObservable<Unit> ForEachAsync<T>(this IAsyncStreamReader<T> stream, Action<T> action)
        {
            return Observable.CreateWithState<Unit, Tuple<IAsyncStreamReader<T>, Action<T>>>(Tuple.Create(stream, action), (state0, observer) =>
            {
                var disp = new MultipleAssignmentDisposable();

                var worker = new AsyncStreamReaderForEachAsync_<T>(disp, state0.Item1, state0.Item2, observer);
                worker.ConsumeNext();

                return disp;
            });
        }

        class AsyncStreamReaderForEachAsync_<T> : IObserver<bool>
        {
            readonly MultipleAssignmentDisposable disp;
            readonly IAsyncStreamReader<T> stream;
            readonly Action<T> action;
            readonly IObserver<Unit> rootObserver;

            int isStopped = 0;

            public AsyncStreamReaderForEachAsync_(MultipleAssignmentDisposable disp, IAsyncStreamReader<T> stream, Action<T> action, IObserver<Unit> rootObserver)
            {
                this.disp = disp;
                this.stream = stream;
                this.action = action;
                this.rootObserver = rootObserver;
            }

            public void ConsumeNext()
            {
                try
                {
                    disp.Disposable = stream.MoveNext().Subscribe(this);
                }
                catch (Exception ex)
                {
                    stream.Dispose();
                    rootObserver.OnError(ex);
                }
            }

            public void OnNext(bool value)
            {
                if (isStopped == 0)
                {
                    if (value == true)
                    {
                        try
                        {
                            action(stream.Current);
                        }
                        catch (Exception ex)
                        {
                            stream.Dispose();
                            rootObserver.OnError(ex);
                            return;
                        }

                        ConsumeNext(); // recursive next
                    }
                    else
                    {
                        rootObserver.OnCompleted();
                    }
                }
            }

            public void OnError(Exception error)
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    stream.Dispose();
                    rootObserver.OnError(error);
                }
            }

            public void OnCompleted()
            {
                // re-use observer.
            }
        }

        public static IObservable<Unit> ForEachAsync<T>(this IAsyncStreamReader<T> stream, Func<T, IObservable<Unit>> asyncAction)
        {
            return Observable.CreateWithState<Unit, Tuple<IAsyncStreamReader<T>, Func<T, IObservable<Unit>>>>(Tuple.Create(stream, asyncAction), (state0, observer) =>
            {
                var disp = new MultipleAssignmentDisposable();

                var worker = new AsyncStreamReaderForEachAsync__<T>(disp, state0.Item1, state0.Item2, observer);
                worker.ConsumeNext();

                return disp;
            });
        }

        class AsyncStreamReaderForEachAsync__<T> : IObserver<bool>
        {
            readonly MultipleAssignmentDisposable disp;
            readonly IAsyncStreamReader<T> stream;
            readonly Func<T, IObservable<Unit>> asyncAction;
            readonly IObserver<Unit> rootObserver;

            int isStopped = 0;

            public AsyncStreamReaderForEachAsync__(MultipleAssignmentDisposable disp, IAsyncStreamReader<T> stream, Func<T, IObservable<Unit>> asyncAction, IObserver<Unit> rootObserver)
            {
                this.disp = disp;
                this.stream = stream;
                this.asyncAction = asyncAction;
                this.rootObserver = rootObserver;
            }

            public void ConsumeNext()
            {
                try
                {
                    disp.Disposable = stream.MoveNext().Subscribe(this);
                }
                catch (Exception ex)
                {
                    stream.Dispose();
                    rootObserver.OnError(ex);
                }
            }

            public void OnNext(bool value)
            {
                if (isStopped == 0)
                {
                    if (value == true)
                    {
                        try
                        {
                            this.disp.Disposable = asyncAction(stream.Current)
                                .Subscribe(_ =>
                                {
                                    ConsumeNext();
                                }, ex => OnError(ex));
                        }
                        catch (Exception ex)
                        {
                            stream.Dispose();
                            rootObserver.OnError(ex);
                            return;
                        }

                        ConsumeNext(); // recursive next
                    }
                    else
                    {
                        rootObserver.OnCompleted();
                    }
                }
            }

            public void OnError(Exception error)
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    stream.Dispose();
                    rootObserver.OnError(error);
                }
            }

            public void OnCompleted()
            {
            }
        }

        public static IObservable<T> AsObservable<T>(this IAsyncStreamReader<T> stream, bool observeOnMainThread = true, IDisposable streamingResult = null)
        {
            var seq = Observable.CreateWithState<T, Tuple<IAsyncStreamReader<T>, IDisposable>>(Tuple.Create(stream, streamingResult), (state, observer) =>
            {
                var disp = new MultipleAssignmentDisposable();
                var b = new AsyncStreamReaderAsObservable_<T>(disp, state.Item1, observer, state.Item2);
                b.ConsumeNext();

                if (state.Item2 == null)
                {
                    return disp;
                }
                else
                {
                    return StableCompositeDisposable.Create(disp, state.Item2);
                }
            });

            return (observeOnMainThread) ? seq.ObserveOnMainThread() : seq;
        }

        class AsyncStreamReaderAsObservable_<T> : IObserver<bool>
        {
            readonly MultipleAssignmentDisposable disp;
            readonly IAsyncStreamReader<T> stream;
            readonly IObserver<T> rootObserver;
            IDisposable streamingResult;

            int isStopped = 0;

            public AsyncStreamReaderAsObservable_(MultipleAssignmentDisposable disp, IAsyncStreamReader<T> stream, IObserver<T> rootObserver, IDisposable streamingResult)
            {
                this.disp = disp;
                this.stream = stream;
                this.rootObserver = rootObserver;
                this.streamingResult = streamingResult ?? Disposable.Empty;
            }

            public void ConsumeNext()
            {
                try
                {
                    disp.Disposable = stream.MoveNext().Subscribe(this);
                }
                catch (Exception ex)
                {
                    stream.Dispose();
                    streamingResult.Dispose();
                    rootObserver.OnError(ex);
                }
            }

            public void OnNext(bool value)
            {
                if (isStopped == 0)
                {
                    if (value == true)
                    {
                        try
                        {
                            rootObserver.OnNext(stream.Current);
                        }
                        catch (Exception ex)
                        {
                            stream.Dispose();
                            streamingResult.Dispose();
                            rootObserver.OnError(ex);
                            return;
                        }

                        ConsumeNext(); // recursive next
                    }
                    else
                    {
                        rootObserver.OnCompleted();
                    }
                }
            }

            public void OnError(Exception error)
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    stream.Dispose();
                    streamingResult.Dispose();
                    rootObserver.OnError(error);
                }
            }

            public void OnCompleted()
            {
                // re-use observer.
            }
        }
    }
}
