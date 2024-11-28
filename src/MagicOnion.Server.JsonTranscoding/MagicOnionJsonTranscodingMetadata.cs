using MagicOnion.Server.Binder;

namespace MagicOnion.Server.JsonTranscoding;

public record MagicOnionJsonTranscodingMetadata(string RoutePath, Type RequestType, Type ResponseType, IMagicOnionGrpcMethod Method);
