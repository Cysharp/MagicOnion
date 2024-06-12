namespace MagicOnion.Server.Hubs;

/// <summary>
/// Attribute for configuring the heartbeat of StreamingHub.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HeartbeatAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value whether the heartbeat feature of StreamingHub is enabled.
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// Gets or sets a StreamingHub heartbeat interval (milliseconds).  If the value is <see cref="System.Threading.Timeout.Infinite"/>, the heartbeat is disabled.
    /// </summary>
    public int Interval { get; set; } = 0;

    /// <summary>
    /// Gets or sets a StreamingHub heartbeat timeout (milliseconds). If the value is <see cref="System.Threading.Timeout.Infinite"/>, the server does not disconnect a client due to timeout.
    /// </summary>
    public int Timeout { get; set; } = 0;

    /// <summary>
    /// Gets or sets an implementation type of <see cref="IStreamingHubHeartbeatMetadataProvider"/>.
    /// </summary>
    public Type? MetadataProvider { get; set; }

    public HeartbeatAttribute(bool enable)
    {
        Enable = enable;
    }

    public HeartbeatAttribute()
    {
        Enable = true;
    }
}
