using System.Threading.Tasks;

namespace MagicOnion
{
    public interface IStreamingHubMarker
    {
    }

    public interface IStreamingHub<TSelf, TReceiver> : IStreamingHubMarker
    {
        Task DisposeAsync();
        Task WaitForDisconnect();
    }
}
