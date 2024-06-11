namespace MagicOnion.Server.Hubs;

[AttributeUsage(AttributeTargets.Class)]
public class HeartbeatAttribute : Attribute
{
    public bool Enable { get; set; }
    public int Interval { get; set; } = 0;
    public int Timeout { get; set; } = 0;
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
