using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Hubs;
using MessagePack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.Redis;

public class RedisGroupRepositoryFactory : IGroupRepositoryFactory
{
    readonly RedisGroupOptions options;
    readonly ILogger logger;

    public RedisGroupRepositoryFactory(IOptionsMonitor<RedisGroupOptions> options, ILogger<RedisGroup> logger)
    {
        this.options = options.CurrentValue;
        this.logger = logger;
    }

    public IGroupRepository CreateRepository(IMagicOnionSerializer messageSerializer)
    {
        return new RedisGroupRepository(messageSerializer, options, logger);
    }
}
