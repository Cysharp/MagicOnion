using System;

namespace MagicOnion.Server.EmbeddedServices
{
    public interface IMagicOnionEmbeddedPing : IService<IMagicOnionEmbeddedPing>
    {
        UnaryResult<double> Ping(DateTime utcSendBegin);
    }

    [Ignore]
    internal class MagicOnionEmbeddedPing : ServiceBase<IMagicOnionEmbeddedPing>, IMagicOnionEmbeddedPing
    {
        public UnaryResult<double> Ping(DateTime utcSendBegin)
        {
            var elapsed = this.Context.Timestamp - utcSendBegin;
            return UnaryResult(elapsed.TotalMilliseconds);
        }
    }
}