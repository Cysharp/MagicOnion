using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    internal interface IStreamingContextInfo
    {
        object ServerStreamingContext { get; }
        void Complete();
    }

    public class StreamingContextInfo<T> : IStreamingContextInfo
    {
        readonly TaskCompletionSource<object> tcs;
        readonly object serverStreamingContext;

        object IStreamingContextInfo.ServerStreamingContext
        {
            get
            {
                return serverStreamingContext;
            }
        }

        internal StreamingContextInfo(TaskCompletionSource<object> tcs, object serverStreamingContext)
        {
            this.tcs = tcs;
            this.serverStreamingContext = serverStreamingContext;
        }

        public TaskAwaiter<ServerStreamingResult<T>> GetAwaiter()
        {
            return tcs.Task.ContinueWith(_ => default(ServerStreamingResult<T>)).GetAwaiter();
        }

        public void Complete()
        {
            tcs.TrySetResult(null);
        }
    }

    public class StreamingContextRepository<TService> : IDisposable
    {
        // (ConcreteServiceType, TResponse) => Func<TService, ServerStreamingContext<TResponse>>;
        static ConcurrentDictionary<Tuple<Type, Type>, Delegate> delegateCache = new ConcurrentDictionary<Tuple<Type, Type>, Delegate>();

        bool isDisposed;
        TService dummyInstance;

        readonly ConcurrentDictionary<string, Tuple<SemaphoreSlim, IStreamingContextInfo>> streamingContext = new ConcurrentDictionary<string, Tuple<SemaphoreSlim, IStreamingContextInfo>>();

        public bool IsDisposed => isDisposed;
        public ConnectionContext ConnectionContext { get; }

        public StreamingContextRepository(ConnectionContext connectionContext)
        {
            this.ConnectionContext = connectionContext;
            connectionContext.ConnectionStatus.Register(() =>
            {
                Dispose();
            });
        }

        public StreamingContextInfo<TResponse> RegisterStreamingMethod<TResponse>(TService self, Func<Task<ServerStreamingResult<TResponse>>> methodSelector)
        {
            if (isDisposed) throw new ObjectDisposedException("StreamingContextRepository", "already disposed(disconnected).");

            if (dummyInstance == null)
            {
                dummyInstance = (TService)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(self.GetType());
            }

            var getServerStreamingContext = delegateCache.GetOrAdd(Tuple.Create(self.GetType(), typeof(TResponse)), x =>
            {
                var methodInfo = x.Item1.GetMethod("GetServerStreamingContext").MakeGenericMethod(typeof(TResponse));

                var arg1 = Expression.Parameter(typeof(TService), "instance");
                var convert = Expression.Convert(arg1, x.Item1);
                var call = Expression.Call(convert, methodInfo);
                var lambda = Expression.Lambda<Func<TService, ServerStreamingContext<TResponse>>>(call, arg1);
                return lambda.Compile();
            });

            var context = (getServerStreamingContext as Func<TService, ServerStreamingContext<TResponse>>).Invoke(self);

            var tcs = new TaskCompletionSource<object>();

            if (context.ServiceContext.GetConnectionContext() != ConnectionContext)
            {
                throw new Exception("TSerivce connection and initialized connection are different.");
            }

            ConnectionContext.ConnectionStatus.Register(state =>
            {
                ((TaskCompletionSource<object>)state).TrySetResult(null);
            }, tcs);

            var info = new StreamingContextInfo<TResponse>(tcs, context);
            var method = $"{typeof(TService).FullName}.{methodSelector.Method.Name}";
            streamingContext[method] = Tuple.Create(new SemaphoreSlim(1, 1), (IStreamingContextInfo)info);
            return info;
        }

        public async Task WriteAsync<TResponse>(Func<TService, string> methodSelector, TResponse value)
        {
            if (isDisposed) throw new ObjectDisposedException("StreamingContextRepository", "already disposed(disconnected).");

            var method = $"{typeof(TService).FullName}.{methodSelector(dummyInstance)}";
            if (streamingContext.TryGetValue(method, out var streamingContextObject))
            {
                try
                {
                    await streamingContextObject.Item1.WaitAsync().ConfigureAwait(false); // wait lock
                    if (isDisposed) return;
                    var context = streamingContextObject.Item2.ServerStreamingContext as ServerStreamingContext<TResponse>;
                    await context.WriteAsync(value).ConfigureAwait(false);
                }
                finally
                {
                    if (!isDisposed)
                    {
                        streamingContextObject.Item1.Release();
                    }
                }
            }
            else
            {
                throw new Exception("Does not exists streaming context. : " + method);
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            // complete all.
            foreach (var item in streamingContext)
            {
                var semaphore = item.Value.Item1;
                while (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
                item.Value.Item2.Complete();
            }
        }
    }
}