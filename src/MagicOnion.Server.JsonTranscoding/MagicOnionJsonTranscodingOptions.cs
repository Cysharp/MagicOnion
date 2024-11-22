using System.Text.Json;

namespace MagicOnion.Server.JsonTranscoding;

public class MagicOnionJsonTranscodingOptions
{
    public string RoutePathPrefix { get; set; } = "/webapi/";
    public bool AllowEnableInNonDevelopmentEnvironment { get; set; } = false;
}
