using MagicOnion;
using MessagePack;

namespace Assets.Scripts.ServerShared.Services
{
    /// <summary>
    /// Client -> Server API
    /// </summary>
    public interface IChatService : IService<IChatService>
    {
        UnaryResult<Nil> SendReportAsync(string message);
    }
}
