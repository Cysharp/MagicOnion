using MagicOnion.Server.Hubs;
using MessagePack;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.Redis;

public class RedisGroupRepositoryFactory : IGroupRepositoryFactory
{
    private readonly RedisGroupOptions options;

    public RedisGroupRepositoryFactory(IOptionsMonitor<RedisGroupOptions> options)
    {
        this.options = options.CurrentValue;
    }

    public IGroupRepository CreateRepository(MessagePackSerializerOptions serializerOptions, IMagicOnionLogger logger)
    {
        return new RedisGroupRepository(serializerOptions, options, logger);
    }
}
