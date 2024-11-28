using Grpc.AspNetCore.Server;
using Grpc.Core;
using MagicOnion.Server.Binder;

namespace MagicOnion.Server.JsonTranscoding;

public class MagicOnionJsonTranscodingServerCallContext(IMagicOnionGrpcMethod method) : ServerCallContext, IServerCallContextFeature
{
    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => throw new NotImplementedException();

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) => throw new NotImplementedException();

    protected override string MethodCore { get; } = $"{method.ServiceName}/{method.MethodName}";
    protected override string HostCore => throw new NotImplementedException();
    protected override string PeerCore => throw new NotImplementedException();
    protected override DateTime DeadlineCore => throw new NotImplementedException();
    protected override Metadata RequestHeadersCore => throw new NotImplementedException();
    protected override CancellationToken CancellationTokenCore => throw new NotImplementedException();
    protected override Metadata ResponseTrailersCore => throw new NotImplementedException();
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    protected override AuthContext AuthContextCore => throw new NotImplementedException();
    public ServerCallContext ServerCallContext => this;
}
