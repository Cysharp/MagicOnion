namespace MagicOnion.Server.JsonTranscoding;

/// <summary>
/// Options for MagicOnion JSON transcoding service.
/// </summary>
public class MagicOnionJsonTranscodingOptions
{
    /// <summary>
    /// Gets or sets the route path prefix for JSON transcoding enabled web API services. Default is <value>/webapi/</value>.
    /// </summary>
    public string RoutePathPrefix { get; set; } = "/webapi/";

    /// <summary>
    /// Gets or sets whether to allow enabling MagicOnion.Server.JsonTranscoding in non-development environment. Default is <see langword="false"/>.
    /// </summary>
    public bool AllowEnableInNonDevelopmentEnvironment { get; set; } = false;
}
