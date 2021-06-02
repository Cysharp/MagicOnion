using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using MicroServer.Shared;
using System;
using System.Threading.Tasks;

namespace MicroServer
{
    public class MessageService : ServiceBase<IMessageService>, IMessageService
    {
        public async UnaryResult<string> SendAsync(string message)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            return $"echo {message}";
        }
    }
}
