using Grpc.Core;
using System;
using UniRx;

namespace MagicOnion
{
    public static class AsyncStreamReaderExtensions
    {
        //TODO:implement Observbale.While
        public static IObservable<Unit> ForEachAsync<T>(this IAsyncStreamReader<T> stream, Action<T> action)
        {
            return RecursiveAction(stream, action);
        }

        static IObservable<Unit> RecursiveAction<T>(IAsyncStreamReader<T> stream, Action<T> action)
        {
            return stream.MoveNext().ContinueWith(x =>
            {
                if (x)
                {
                    action(stream.Current);
                    return RecursiveAction(stream, action);
                }
                else
                {
                    stream.Dispose();
                    return Observable.ReturnUnit();
                }
            });
        }

        public static IObservable<Unit> ForEachAsync<T>(this IAsyncStreamReader<T> stream, Func<T, IObservable<Unit>> asyncAction)
        {
            return RecursiveActionAsync(stream, asyncAction);
        }

        static IObservable<Unit> RecursiveActionAsync<T>(IAsyncStreamReader<T> stream, Func<T, IObservable<Unit>> asyncAction)
        {
            return stream.MoveNext().ContinueWith(x =>
            {
                if (x)
                {
                    return asyncAction(stream.Current)
                        .ContinueWith(_ => RecursiveActionAsync(stream, asyncAction));
                }
                else
                {
                    stream.Dispose();
                    return Observable.ReturnUnit();
                }
            });
        }

        // TODO:needs strict implementation
        public static IObservable<T> AsObservable<T>(this IAsyncStreamReader<T> stream)
        {
            var subject = new Subject<T>();
            stream.ForEachAsync(x => subject.OnNext(x)); // cancellation!
            return subject;
        }
    }
}
