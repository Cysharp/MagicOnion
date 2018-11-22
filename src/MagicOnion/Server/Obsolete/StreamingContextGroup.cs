using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    [Obsolete("Use Hub instead.")]
    public class StreamingContextGroup<TKey, TStreamingService>
    {
        readonly ConcurrentDictionary<TKey, StreamingContextRepository<TStreamingService>> repositories;
        readonly IEqualityComparer<TKey> comparer;

        public int Count
        {
            get
            {
                return repositories.Count;
            }
        }
        public IEnumerable<TKey> Keys
        {
            get
            {
                return repositories.Keys;
            }
        }

        public IEnumerable<KeyValuePair<TKey, StreamingContextRepository<TStreamingService>>> KeyValues
        {
            get
            {
                return repositories.AsEnumerable();
            }
        }

        [Obsolete("Use Hub instead.")]
        public StreamingContextGroup()
            : this(EqualityComparer<TKey>.Default)
        {
        }

        [Obsolete("Use Hub instead.")]
        public StreamingContextGroup(IEqualityComparer<TKey> comparer)
        {
            this.repositories = new ConcurrentDictionary<TKey, StreamingContextRepository<TStreamingService>>(comparer);
            this.comparer = comparer;
        }

        public void Add(TKey key, StreamingContextRepository<TStreamingService> repository)
        {
            if (repository.IsDisposed) return;
            repositories[key] = repository;

            // when detect disconnected, automatically remove.
            repository.ConnectionContext.ConnectionStatus.Register(() =>
            {
                Remove(key);
            });
        }

        // can't return value because if automatically remove raises at first, user can't take it.
        public void Remove(TKey key)
        {
            StreamingContextRepository<TStreamingService> value;
            repositories.TryRemove(key, out value);
        }

        public StreamingContextRepository<TStreamingService> Get(TKey key)
        {
            StreamingContextRepository<TStreamingService> v;
            return repositories.TryGetValue(key, out v) ? v : null;
        }

        public IEnumerable<StreamingContextRepository<TStreamingService>> All()
        {
            return repositories.Values;
        }

        public IEnumerable<StreamingContextRepository<TStreamingService>> AllExcept(TKey exceptKey)
        {
            return repositories.Where(x => !comparer.Equals(x.Key, exceptKey)).Select(x => x.Value);
        }

        public IEnumerable<StreamingContextRepository<TStreamingService>> AllExcept(params TKey[] exceptKeys)
        {
            return AllExcept(exceptKeys.AsEnumerable());
        }

        public IEnumerable<StreamingContextRepository<TStreamingService>> AllExcept(IEnumerable<TKey> exceptKeys)
        {
            var set = new HashSet<TKey>(exceptKeys, comparer);
            return repositories.Where(x => !set.Equals(x.Key)).Select(x => x.Value);
        }

        public async Task BroadcastToAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, IEnumerable<TKey> includeKeys, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(includeKeys.Select(x => this.Get(x)).Where(x => x != null).Select(x =>
                {
                    return AwaitErrorHandling(x.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in includeKeys.Select(x => this.Get(x)).Where(x => x != null))
                {
                    await AwaitErrorHandling(item.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        public async Task BroadcastAllAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(repositories.Values.Select(x =>
                {
                    return AwaitErrorHandling(x.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in repositories.Values)
                {
                    await AwaitErrorHandling(item.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        public async Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, TKey exceptKey, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(AllExcept(exceptKey).Select(x =>
                {
                    return AwaitErrorHandling(x.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in AllExcept(exceptKey))
                {
                    await AwaitErrorHandling(item.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        public async Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, IEnumerable<TKey> exceptKeys, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(AllExcept(exceptKeys).Select(x =>
                {
                    return AwaitErrorHandling(x.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in AllExcept(exceptKeys))
                {
                    await AwaitErrorHandling(item.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        async Task AwaitErrorHandling(Task task, bool ignoreError)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (RpcException ex)
            {
                if (ignoreError)
                {
                    GrpcEnvironment.Logger.Error(ex, "logged but ignore error from StreamingContextGroup.BroadcastAll");
                }
                else
                {
                    throw;
                }
            }
        }
    }

    [Obsolete("Use Hub instead.")]
    public class StreamingContextGroup<TKey, TValue, TStreamingService>
    {
        readonly ConcurrentDictionary<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>> repositories;
        readonly IEqualityComparer<TKey> comparer;

        public int Count
        {
            get
            {
                return repositories.Count;
            }
        }
        public IEnumerable<TKey> Keys
        {
            get
            {
                return repositories.Keys;
            }
        }

        public IEnumerable<KeyValuePair<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>>> KeyValues
        {
            get
            {
                return repositories.AsEnumerable();
            }
        }

        [Obsolete("Use Hub instead.")]
        public StreamingContextGroup()
                : this(EqualityComparer<TKey>.Default)
        {
        }

        [Obsolete("Use Hub instead.")]
        public StreamingContextGroup(IEqualityComparer<TKey> comparer)
        {
            this.repositories = new ConcurrentDictionary<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>>(comparer);
            this.comparer = comparer;
        }

        public void Add(TKey key, TValue value, StreamingContextRepository<TStreamingService> repository)
        {
            if (repository.IsDisposed) return;
            repositories[key] = Tuple.Create(value, repository);
        }

        public Tuple<TValue, StreamingContextRepository<TStreamingService>> Remove(TKey key)
        {
            Tuple<TValue, StreamingContextRepository<TStreamingService>> value;
            return repositories.TryRemove(key, out value)
                ? value
                : null;
        }

        public Tuple<TValue, StreamingContextRepository<TStreamingService>> Get(TKey key)
        {
            Tuple<TValue, StreamingContextRepository<TStreamingService>> v;
            return repositories.TryGetValue(key, out v) ? v : null;
        }

        public TValue GetValue(TKey key)
        {
            var value = Get(key);
            return (value != null) ? value.Item1 : default(TValue);
        }

        public IEnumerable<Tuple<TValue, StreamingContextRepository<TStreamingService>>> All()
        {
            return repositories.Values;
        }

        public IEnumerable<Tuple<TValue, StreamingContextRepository<TStreamingService>>> AllExcept(TKey exceptKey)
        {
            return repositories.Where(x => !comparer.Equals(x.Key, exceptKey)).Select(x => x.Value);
        }

        public IEnumerable<Tuple<TValue, StreamingContextRepository<TStreamingService>>> AllExcept(params TKey[] exceptKeys)
        {
            var set = new HashSet<TKey>(exceptKeys, comparer);
            return repositories.Where(x => !set.Equals(x.Key)).Select(x => x.Value);
        }

        public async Task BroadcastAllAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(repositories.Values.Select(x =>
                {
                    return AwaitErrorHandling(x.Item2.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in repositories.Values)
                {
                    await AwaitErrorHandling(item.Item2.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        public async Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, TKey exceptKey, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(AllExcept(exceptKey).Select(x =>
                {
                    return AwaitErrorHandling(x.Item2.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in AllExcept(exceptKey))
                {
                    await AwaitErrorHandling(item.Item2.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        public async Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, TKey[] exceptKeys, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(AllExcept(exceptKeys).Select(x =>
                {
                    return AwaitErrorHandling(x.Item2.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in AllExcept(exceptKeys))
                {
                    await AwaitErrorHandling(item.Item2.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        async Task AwaitErrorHandling(Task task, bool ignoreError)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (RpcException ex)
            {
                if (ignoreError)
                {
                    GrpcEnvironment.Logger.Error(ex, "logged but ignore error from StreamingContextGroup.BroadcastAll");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}