namespace MagicOnion.Server.OpenTelemetry.Internal
{
    /// <summary>
    /// OpenTelemetry Tag Keys
    /// </summary>
    internal static class SemanticConventions
    {
        // tag spec: https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/rpc.md#grpc
        public const string AttributeServiceName = "service.name";
        public const string AttributeException = "exception";

        public const string AttributeHttpHost = "http.host";
        public const string AttributeHttpUrl = "http.url";
        public const string AttributeHttpUserAgent = "http.user_agent";

        public const string AttributeRpcGrpcMethod = "rpc.grpc.method";
        public const string AttributeRpcGrpcStatusCode = "rpc.grpc.status_code";
        public const string AttributeRpcGrpcStatusDetail = "rpc.grpc.status_detail";
        public const string AttributeRpcSystem = "rpc.system";
        public const string AttributeRpcService = "rpc.service";
        public const string AttributeRpcMethod = "rpc.method";

        public const string AttributeMessageId = "message.id";
        public const string AttributeMessageCompressedSize = "message.compressed_size";
        public const string AttributeMessageUncompressedSize = "message.uncompressed_size";

        public const string AttributeMagicOnionPeerName = "magiconion.peer.ip";
        public const string AttributeMagicOnionAuthEnabled = "magiconion.auth.enabled";
        public const string AttributeMagicOnionAuthPeerAuthenticated = "magiconion.auth.peer_authenticated";
    }
}
