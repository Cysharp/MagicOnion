using Cysharp.Runtime.Multicast;
using Cysharp.Runtime.Multicast.Distributed.Redis;
using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionServerBuilderRedisExtensions
{
    public static IMagicOnionServerBuilder UseRedisGroup(this IMagicOnionServerBuilder builder, Action<RedisGroupOptions> configure, bool registerAsDefault = false)
    {
        if (registerAsDefault)
        {
            builder.Services.RemoveAll<IMulticastGroupProvider>();
            builder.Services.TryAddSingleton<IMulticastGroupProvider, RedisGroupProvider>();
        }
        builder.Services.Configure<RedisGroupOptions>(configure);

        return builder;
    }
}
