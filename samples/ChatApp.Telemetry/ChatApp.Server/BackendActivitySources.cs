using System.Diagnostics;

namespace ChatApp.Server
{
    public static class BackendActivitySources
    {
        public static readonly string[] ExtraActivitySourceNames = new[] { "Chatapp.Server.S2S", "MySQL", "Redis" };

        public static readonly ActivitySource S2sActivitySource = new ActivitySource("Chatapp.Server.S2S");
        public static readonly ActivitySource MySQLActivitySource = new ActivitySource("MySQL");
        public static readonly ActivitySource RedisActivitySource = new ActivitySource("Redis");
    }
}
