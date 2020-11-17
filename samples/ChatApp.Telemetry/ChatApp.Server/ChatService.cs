using ChatApp.Shared.Services;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChatApp.Server
{
    public class ChatService : ServiceBase<IChatService>, IChatService
    {
        private ActivitySource mysqlActivitySource;
        private ILogger logger;

        public ChatService(ILogger<ChatService> logger, BackendActivitySources backendActivity)
        {
            this.mysqlActivitySource = backendActivity.Get("mysql");
            this.logger = logger;
        }

        public async UnaryResult<Nil> GenerateException(string message)
        {
            var ex = new System.NotImplementedException();
            // dummy external operation.
            using (var activity = mysqlActivitySource.StartActivity("db:errors/insert", ActivityKind.Internal))
            {
                // this is sample. use orm or any safe way.
                activity.SetTag("table", "errors");
                activity.SetTag("query", $"INSERT INTO rooms VALUES ('{ex.Message}', '{ex.StackTrace}');");
                await Task.Delay(TimeSpan.FromMilliseconds(2));
            }
            throw ex;
        }

        public UnaryResult<Nil> SendReportAsync(string message)
        {
            logger.LogDebug($"{message}");

            return UnaryResult(Nil.Default);
        }
    }
}
