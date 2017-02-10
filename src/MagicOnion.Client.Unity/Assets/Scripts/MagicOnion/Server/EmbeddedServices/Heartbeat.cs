using System;
using UniRx;

namespace MagicOnion.Server.EmbeddedServices
{
    public interface IMagicOnionEmbeddedHeartbeat : IService<IMagicOnionEmbeddedHeartbeat>
    {
        IObservable<DuplexStreamingResult<bool, bool>> Connect();
    }
}