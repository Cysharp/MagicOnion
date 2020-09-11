using ChatApp.Shared.Services;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ChatApp.Server
{
    public class ChatService : ServiceBase<IChatService>, IChatService
    {
        private ActivitySource activitySource;

        public ChatService(ActivitySource activitySource)
        {
            this.activitySource = activitySource;
        }

        public async UnaryResult<Nil> GenerateException(string message)
        {
            var ex = new System.NotImplementedException();
            // dummy external operation.
            using (var activity = activitySource.StartActivity("db:errors/insert", ActivityKind.Internal))
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
            Logger.Debug($"{message}");

            return UnaryResult(Nil.Default);
        }
    }
}
