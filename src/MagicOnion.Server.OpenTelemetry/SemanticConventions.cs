namespace MagicOnion.Server.OpenTelemetry
{
    internal static class SemanticConventions
    {
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

        public const string AttributeMessageType = "message.type";
        public const string AttributeMessageId = "message.id";
        public const string AttributeMessageCompressedSize = "message.compressed_size";
        public const string AttributeMessageUncompressedSize = "message.uncompressed_size";

        public const string AttributeMagicOnionPeerName = "magiconion.peer.ip";
        public const string AttributeMagicOnionAuthEnabled = "magiconion.auth.enabled";
        public const string AttributeMagicOnionAuthPeerAuthenticated = "magiconion.auth.peer_authenticated";
    }
}
