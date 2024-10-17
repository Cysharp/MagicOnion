namespace MagicOnion.Server.Internal;

internal interface IStreamingHubBase
{
    /// <summary>
    /// Process DuplexStreaming and start StreamingHub processing.
    /// DO NOT change this name, as it is used as the name to be exposed as gRPC DuplexStreaming.
    /// </summary>
    /// <returns></returns>
    Task Connect();
}
