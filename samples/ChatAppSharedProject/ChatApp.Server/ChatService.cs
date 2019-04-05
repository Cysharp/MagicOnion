using ChatApp.Shared;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;

namespace ChatApp.Server
{
    public class ChatService : ServiceBase<IChatService>, IChatService
    {
        public UnaryResult<Nil> SendReportAsync(string message)
        {
            Logger.Debug($"{message}");

            return UnaryResult(Nil.Default);
        }
    }
}
