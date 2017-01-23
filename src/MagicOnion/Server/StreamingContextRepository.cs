using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        bool isDisposed;
        TService dummyInstance;

        readonly ConcurrentDictionary<MethodInfo, Tuple<SemaphoreSlim, IStreamingContextInfo>> streamingContext = new ConcurrentDictionary<MethodInfo, Tuple<SemaphoreSlim, IStreamingContextInfo>>();

        public bool IsDisposed => isDisposed;

        public StreamingContextInfo<TResponse> RegisterStreamingMethod<TResponse>(TService self, Func<Task<ServerStreamingResult<TResponse>>> methodSelector)
        {
            if (dummyInstance == null)
            {
                dummyInstance = (TService)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(self.GetType());
            }

            // TODO:no reflection
            var context = (ServerStreamingContext<TResponse>)self.GetType().GetMethod("GetServerStreamingContext")
                .MakeGenericMethod(typeof(TResponse))
                .Invoke(self, null);

            var tcs = new TaskCompletionSource<object>();

            context.ServiceContext.GetConnectionContext().ConnectionStatus.Register(state =>
            {
                ((TaskCompletionSource<object>)state).TrySetResult(null);
            }, tcs);

            var info = new StreamingContextInfo<TResponse>(tcs, context);
            streamingContext[methodSelector.Method] = Tuple.Create(new SemaphoreSlim(1, 1), (IStreamingContextInfo)info);
            return info;
        }

        public async Task WriteAsync<TResponse>(Func<TService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value)
        {
            Tuple<SemaphoreSlim, IStreamingContextInfo> streamingContextObject;
            if (streamingContext.TryGetValue(methodSelector(dummyInstance).Method, out streamingContextObject))
            {
                await streamingContextObject.Item1.WaitAsync().ConfigureAwait(false); // wait lock
                if (isDisposed) return;
                var context = streamingContextObject.Item2.ServerStreamingContext as ServerStreamingContext<TResponse>;
                await context.WriteAsync(value).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("Does not exists streaming context. :" + methodSelector.Method.Name);
            }
        }

        public void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException("StreamingContextRepository");
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
