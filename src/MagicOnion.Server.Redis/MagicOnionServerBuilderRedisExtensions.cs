using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionServerBuilderRedisExtensions
{
    public static IMagicOnionServerBuilder UseRedisGroupRepository(this IMagicOnionServerBuilder builder, Action<RedisGroupOptions> configure, bool registerAsDefault = false)
    {
        if (registerAsDefault)
        {
            builder.Services.RemoveAll<IGroupRepositoryFactory>();
            builder.Services.TryAddSingleton<IGroupRepositoryFactory, RedisGroupRepositoryFactory>();

        }
        builder.Services.Configure<RedisGroupOptions>(configure);

        return builder;
    }
}
