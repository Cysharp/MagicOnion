using MessagePack;

namespace MagicOnion.Server.JsonTranscoding;

public class MagicOnionJsonTranscodingOptions
{
    public MessagePackSerializerOptions? MessagePackSerializerOptions { get; set; }
}
