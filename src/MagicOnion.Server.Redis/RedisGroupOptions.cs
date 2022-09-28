using StackExchange.Redis;
using System;

namespace MagicOnion.Server.Redis;

public class RedisGroupOptions
{
    public ConnectionMultiplexer ConnectionMultiplexer { get; set; }
    public int Db { get; set; } = -1;
}
