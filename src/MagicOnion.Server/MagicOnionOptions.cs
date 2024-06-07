using MessagePack;
using MagicOnion.Server.Filters;
using MagicOnion.Serialization;
using MagicOnion.Serialization.MessagePack;

namespace MagicOnion.Server;

public class MagicOnionOptions
{
    /// <summary>
    /// Gets and sets the serializer that serializes the message. The default serializer is MagicOnionMessageSerializer.Default.
    /// </summary>
    public IMagicOnionSerializerProvider MessageSerializer { get; set; }

    /// <summary>
    /// If true, MagicOnion handles exception own self and send to message. If false, propagate to gRPC engine. Default is false.
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
    }
}
