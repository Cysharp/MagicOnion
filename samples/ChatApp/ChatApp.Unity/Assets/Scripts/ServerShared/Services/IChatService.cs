using MagicOnion;
using MessagePack;

namespace ChatApp.Shared.Services
{
    /// <summary>
    /// Client -> Server API
    /// </summary>
    public interface IChatService : IService<IChatService>
    {
        UnaryResult<Nil> SendReportAsync(string message);
    }
}
