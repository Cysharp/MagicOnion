using MagicOnion;
using MessagePack;

namespace MicroServer.Shared
{
    public interface IMessageService : IService<IMessageService>
    {
        UnaryResult<string> SendAsync(string message);
    }
}
