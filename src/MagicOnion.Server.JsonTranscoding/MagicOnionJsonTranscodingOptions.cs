using System.Text.Json;

namespace MagicOnion.Server.JsonTranscoding;

public class MagicOnionJsonTranscodingOptions
{
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
    public string RoutePathPrefix { get; set; } = "/_/";
}
