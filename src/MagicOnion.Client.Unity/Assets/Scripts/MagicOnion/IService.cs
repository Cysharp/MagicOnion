using Grpc.Core;
using System;

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
        TSelf WithCancellationToken(GrpcCancellationToken cancellationToken);
        TSelf WithHost(string host);
    }
}
