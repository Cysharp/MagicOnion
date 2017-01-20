using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace MagicOnion.Server
{

    // TODO:Impl No TValue.
    public class StreamingContextGroup<TKey, TStreamingService>
    {
    }

    public class StreamingContextGroup<TKey, TValue, TStreamingService>
    {
        ImmutableDictionary<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>> repositories;

        public int Count
        {
            get
            {
                return repositories.Count;
            }
        }

        public IEnumerable<TKey> Keys()
        {
            return repositories.Keys;
        }

        public IEnumerable<KeyValuePair<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>>> KeyValues()
        {
            return repositories.AsEnumerable();
        }

        public StreamingContextGroup()
        {
            repositories = ImmutableDictionary<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>>.Empty;
        }

        public StreamingContextGroup(IEqualityComparer<TKey> comparer)
        {
            repositories = ImmutableDictionary<TKey, Tuple<TValue, StreamingContextRepository<TStreamingService>>>.Empty.WithComparers(comparer);
        }

        public void Add(TKey key, TValue value, StreamingContextRepository<TStreamingService> repository)
        {
            if (repository.IsDisposed) return;
            ImmutableInterlocked.Update(ref repositories, (x, y) => x.Add(y.Item1, Tuple.Create(y.Item2, y.Item3)), Tuple.Create(key, value, repository));
        }

        public void Remove(TKey key)
        {
            ImmutableInterlocked.Update(ref repositories, (x, y) => x.Remove(y), key);
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
            var comparer = repositories.KeyComparer;
            return repositories.Where(x => !comparer.Equals(x.Key, exceptKey)).Select(x => x.Value);
        }

        public IEnumerable<Tuple<TValue, StreamingContextRepository<TStreamingService>>> AllExcept(params TKey[] exceptKeys)
        {
            var comparer = repositories.KeyComparer;
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
