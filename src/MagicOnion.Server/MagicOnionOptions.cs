using MessagePack;
using MagicOnion.Server.Filters;

namespace MagicOnion.Server;

public class MagicOnionOptions
{
    /// <summary>
    /// MessagePack serialization resolver. Default is used ambient default(MessagePackSerializer.DefaultOptions).
    /// </summary>
    public MessagePackSerializerOptions SerializerOptions { get; set; }

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
    /// Constructor can handle only error detail. If you want to set the other options, you can use object initializer. 
    /// </summary>
    public MagicOnionOptions()
    {
        this.IsReturnExceptionStackTraceInErrorDetail = false;
        this.SerializerOptions = MessagePackSerializer.DefaultOptions;
        this.GlobalFilters = new List<MagicOnionServiceFilterDescriptor>();
        this.GlobalStreamingHubFilters = new List<StreamingHubFilterDescriptor>();
        this.EnableCurrentContext = false;
    }
}
