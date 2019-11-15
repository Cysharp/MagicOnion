using System.Threading.Tasks;

namespace MagicOnion
{
    public interface IStreamingHubMarker
    {
    }

    public interface IStreamingHub<TSelf, TReceiver> : IStreamingHubMarker, IService<TSelf>
    {
        TSelf FireAndForget(); // if changed to get-only property, intellisense does not work on VS2017.
        Task DisposeAsync();
        Task WaitForDisconnect();
    }
}
