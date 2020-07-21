using MagicOnion.Server.Hubs;
using MessagePack;
using System.Collections.Generic;

namespace MagicOnion.Server
{
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
        /// Set the diagnostics info logger.
        /// </summary>
        public IMagicOnionLogger MagicOnionLogger { get; set; }

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
        /// Default GroupRepository factory for StreamingHub.
        /// </summary>
        public IGroupRepositoryFactory DefaultGroupRepositoryFactory { get; set; }

        /// <summary>
        /// Constructor can handle only error detail. If you want to set the other options, you can use object initializer. 
        /// </summary>
        /// <param name="isReturnExceptionStackTraceInErrorDetail">true, when method body throws exception send to client exception.ToString message. It is useful for debugging. Default is false.</param>
        public MagicOnionOptions(bool isReturnExceptionStackTraceInErrorDetail = false)
        {
            this.IsReturnExceptionStackTraceInErrorDetail = isReturnExceptionStackTraceInErrorDetail;
            this.SerializerOptions = MessagePackSerializer.DefaultOptions;
            this.MagicOnionLogger = new NullMagicOnionLogger();
            this.GlobalFilters = new List<MagicOnionServiceFilterDescriptor>();
            this.GlobalStreamingHubFilters = new List<StreamingHubFilterDescriptor>();
            this.DefaultGroupRepositoryFactory = new ImmutableArrayGroupRepositoryFactory();
            this.EnableCurrentContext = false;
        }
    }
}