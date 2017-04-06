using MessagePack;
using UniRx;

namespace MagicOnion.Server.EmbeddedServices
{
    public interface IMagicOnionEmbeddedHeartbeat : IService<IMagicOnionEmbeddedHeartbeat>
    {
        IObservable<DuplexStreamingResult<Nil, Nil>> Connect();
    }
}