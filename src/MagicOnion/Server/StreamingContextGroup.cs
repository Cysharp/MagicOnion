using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
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

        public StreamingContextGroup()
            : this(EqualityComparer<TKey>.Default)
        {
        }

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

        public IEnumerable<StreamingContextRepository<TStreamingService>> Get(params TKey[] keys)
        {
            return keys.Select(x => this.Get(x)).Where(x => x != null);
        }

        public IEnumerable<StreamingContextRepository<TStreamingService>> Get(IEnumerable<TKey> keys)
        {
            return keys.Select(x => this.Get(x)).Where(x => x != null);
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

        public Task BroadcastToAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, IEnumerable<TKey> includeKeys, bool parallel = true, bool ignoreError = true)
        {
            return Get(includeKeys).BroadcastAsync(methodSelector, value, parallel, ignoreError);
        }

        public Task BroadcastAllAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, bool parallel = true, bool ignoreError = true)
        {
            return All().BroadcastAsync(methodSelector, value, parallel, ignoreError);
        }

        public Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, TKey exceptKey, bool parallel = true, bool ignoreError = true)
        {
            return AllExcept(exceptKey).BroadcastAsync(methodSelector, value, parallel, ignoreError);
        }

        public Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, IEnumerable<TKey> exceptKeys, bool parallel = true, bool ignoreError = true)
        {
            return AllExcept(exceptKeys).BroadcastAsync(methodSelector, value, parallel, ignoreError);
        }
    }

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

        public StreamingContextGroup()
            : this(EqualityComparer<TKey>.Default)
        {
        }

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

        public IEnumerable<Tuple<TValue, StreamingContextRepository<TStreamingService>>> Get(params TKey[] keys)
        {
            return keys.Select(x => this.Get(x)).Where(x => x != null);
        }

        public IEnumerable<Tuple<TValue, StreamingContextRepository<TStreamingService>>> Get(IEnumerable<TKey> keys)
        {
            return keys.Select(x => this.Get(x)).Where(x => x != null);
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

        public Task BroadcastToAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, IEnumerable<TKey> includeKeys, bool parallel = true, bool ignoreError = true)
        {
            return Get(includeKeys)
                .Select(x => x.Item2)
                .BroadcastAsync(methodSelector, value, parallel, ignoreError);
        }

        public Task BroadcastAllAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, bool parallel = true, bool ignoreError = true)
        {
            return All()
                .Select(x => x.Item2)
                .BroadcastAsync(methodSelector, value, parallel, ignoreError);
        }

        public Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, TKey exceptKey, bool parallel = true, bool ignoreError = true)
        {
            return AllExcept(exceptKey)
                .Select(x => x.Item2)
                .BroadcastAsync(methodSelector, value, parallel, ignoreError);
        }

        public Task BroadcastAllExceptAsync<TResponse>(Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, TKey[] exceptKeys, bool parallel = true, bool ignoreError = true)
        {
            return AllExcept(exceptKeys)
                .Select(x => x.Item2)
                .BroadcastAsync(methodSelector, value, parallel, ignoreError);
        }
    }

    public static class StreamingContextGroupExtensions
    {
        public static async Task BroadcastAsync<TStreamingService, TResponse>(this IEnumerable<StreamingContextRepository<TStreamingService>> repositories, Func<TStreamingService, Func<Task<ServerStreamingResult<TResponse>>>> methodSelector, TResponse value, bool parallel = true, bool ignoreError = true)
        {
            if (parallel)
            {
                await Task.WhenAll(repositories.Select(x =>
                {
                    return AwaitErrorHandling(x.WriteAsync(methodSelector, value), ignoreError);
                })).ConfigureAwait(false);
            }
            else
            {
                foreach (var item in repositories)
                {
                    await AwaitErrorHandling(item.WriteAsync(methodSelector, value), ignoreError).ConfigureAwait(false);
                }
            }
        }

        private async static Task AwaitErrorHandling(Task task, bool ignoreError)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (RpcException ex)
            {
                if (ignoreError)
                {
                    GrpcEnvironment.Logger.Error(ex, "logged but ignore error from StreamingContextGroup.Broadcast");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}