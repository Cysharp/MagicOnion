using System;

namespace MagicOnion.Server.EmbeddedServices
{
    public interface IMagicOnionEmbeddedPing : IService<IMagicOnionEmbeddedPing>
    {
        UnaryResult<double> Ping(DateTime utcSendBegin);
    }
}