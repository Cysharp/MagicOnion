using MessagePack;
using MagicOnion.Server.Filters;
using MagicOnion.Serialization;
using MagicOnion.Serialization.MessagePack;

namespace MagicOnion.Server;

public class MagicOnionOptions
{
    /// <summary>
    /// Gets and sets the serializer that serializes the message. The default serializer is <see cref="MagicOnionSerializerProvider.Default"/>.
    /// </summary>
    public IMagicOnionSerializerProvider MessageSerializer { get; set; }

    /// <summary>
    /// If true, MagicOnion handles exception own self and send to message. If false, propagate to gRPC engine. Default is <see keyword="false"/>.
    /// </summary>
    public bool IsReturnExceptionStackTraceInErrorDetail { get; set; }

    /// <summary>
    /// Enable ServiceContext.Current option by AsyncLocal.
    /// </summary>
    public bool EnableCurrentContext { get; set; }

    /// <summary>
    /// Global MagicOnion filters.
    /// </summary>
    public IList<MagicOnionServiceFilterDescriptor> GlobalFilters { get; set; }

    /// <summary>
    /// Global StreamingHub filters.
    /// </summary>
    public IList<StreamingHubFilterDescriptor> GlobalStreamingHubFilters { get; set; }

    /// <summary>
    /// Gets or sets the default timeout duration of client results. Default is 5 seconds.
    /// </summary>
    public TimeSpan ClientResultsDefaultTimeout { get; set; }

    /// <summary>
    /// Gets or sets a value whether the heartbeat feature of StreamingHub is enabled. Default is <see keyword="false"/>.
    /// </summary>
    public bool EnableStreamingHubHeartbeat { get; set; }

    /// <summary>
    /// Gets or sets a StreamingHub heartbeat interval. Default is <see keyword="null"/>. If the value is <see keyword="null"/>, the heartbeat is disabled.
    /// </summary>
    public TimeSpan? StreamingHubHeartbeatInterval { get; set; }

    /// <summary>
    /// Gets or sets a StreamingHub heartbeat timeout. Default is <see keyword="null"/>. If the value is <see keyword="null"/>, the server does not disconnect a client due to timeout.
    /// </summary>
    public TimeSpan? StreamingHubHeartbeatTimeout { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="System.TimeProvider"/> used internally by MagicOnion.
    /// </summary>
    public TimeProvider? TimeProvider { get; set; }

    /// <summary>
    /// Constructor can handle only error detail. If you want to set the other options, you can use object initializer.
    /// </summary>
    public MagicOnionOptions()
    {
        this.IsReturnExceptionStackTraceInErrorDetail = false;
        this.MessageSerializer = (MagicOnionSerializerProvider.Default is MessagePackMagicOnionSerializerProvider provider)
            ? provider.WithEnableFallback(true) // If the default provider is MessagePack, we need to enable fallback options for optional parameters.
            : MagicOnionSerializerProvider.Default;
        this.GlobalFilters = new List<MagicOnionServiceFilterDescriptor>();
        this.GlobalStreamingHubFilters = new List<StreamingHubFilterDescriptor>();
        this.EnableCurrentContext = false;
        this.ClientResultsDefaultTimeout = TimeSpan.FromSeconds(5);
        this.EnableStreamingHubHeartbeat = false;
        this.StreamingHubHeartbeatInterval = null;
        this.StreamingHubHeartbeatTimeout = null;
        this.TimeProvider = null;
    }
}
