using Grpc.Core;
using System;

namespace MagicOnion
{
    // used for MagicOnionEngine assembly scan for boostup analyze speed.
    public interface __IServiceMarker
    {

    }

    public interface IService<TSelf> : __IServiceMarker
    {
        TSelf WithOptions(CallOptions option);
        TSelf WithHeaders(Metadata headers);
        TSelf WithDeadline(DateTime deadline);
        TSelf WithCancellationToken(GrpcCancellationToken cancellationToken);
        TSelf WithHost(string host);
    }
}
