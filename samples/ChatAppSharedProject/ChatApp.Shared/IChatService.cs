using MagicOnion;
using MessagePack;

namespace ChatApp.Shared
{
    /// <summary>
    /// Client -> Server API
    /// </summary>
    public interface IChatService : IService<IChatService>
    {
        UnaryResult<Nil> SendReportAsync(string message);
    }
}
