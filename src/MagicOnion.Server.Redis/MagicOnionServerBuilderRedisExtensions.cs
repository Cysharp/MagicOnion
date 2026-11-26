using Cysharp.Runtime.Multicast;
using Cysharp.Runtime.Multicast.Distributed.Redis;
using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionServerBuilderRedisExtensions
{
    /// <summary>Configures Redis-backed multicast groups for StreamingHub.</summary>
    /// <param name="builder">The MagicOnion server builder.</param>
    /// <param name="configure">The action that configures the Redis connection and channel options.</param>
    /// <param name="registerAsDefault">Whether to use Redis for groups without an explicit provider.</param>
    /// <returns>The server builder.</returns>
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
