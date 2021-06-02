using System.Diagnostics;

namespace ChatApp.Server
{
    public static class BackendActivitySources
    {
        public static readonly string[] ExtraActivitySourceNames = new[] { "chatapp.server.s2s", "mysql", "redis" };

        public static readonly ActivitySource S2sActivitySource = new ActivitySource("chatapp.server.s2s");
        public static readonly ActivitySource MySQLActivitySource = new ActivitySource("mysql");
        public static readonly ActivitySource RedisActivitySource = new ActivitySource("redis");
    }
}
