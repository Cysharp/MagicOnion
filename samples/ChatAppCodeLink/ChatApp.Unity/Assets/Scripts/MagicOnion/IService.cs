using Grpc.Core;
using System;
using System.Threading;

namespace MagicOnion
{
    // used for MagicOnionEngine assembly scan for boostup analyze speed.
    public interface IServiceMarker
    {

    }

    public interface IService<TSelf> : IServiceMarker
    {
        TSelf WithOptions(CallOptions option);
        TSelf WithHeaders(Metadata headers);
        TSelf WithDeadline(DateTime deadline);
        TSelf WithCancellationToken(CancellationToken cancellationToken);
        TSelf WithHost(string host);
    }
}
