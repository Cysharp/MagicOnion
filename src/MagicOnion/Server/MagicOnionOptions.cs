using MagicOnion.Server.Hubs;
using MessagePack;
using System.Collections.Generic;

namespace MagicOnion.Server
{
    public class MagicOnionOptions
    {
        /// <summary>
        /// MessagePack serialization resolver. Default is used ambient default(MessagePackSerialzier.Default).
        /// </summary>
        public IFormatterResolver FormatterResolver { get; set; }

        /// <summary>
        /// If true, MagicOnion handles exception ownself and send to message. If false, propagate to gRPC engine. Default is false.
        /// </summary>
        public bool IsReturnExceptionStackTraceInErrorDetail { get; set; }

        /// <summary>
        /// Set the diagnostics info logger.
        /// </summary>
        public IMagicOnionLogger MagicOnionLogger { get; set; }

        /// <summary>
        /// Disable embedded service(ex:heartbeat), default is false.
        /// </summary>
        public bool DisableEmbeddedService { get; set; }

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
        /// Add the extra typed option.
        /// </summary>
        public IServiceLocator ServiceLocator { get; set; }

        /// <summary>
        /// Service instance activator.
        /// </summary>
        public IServiceActivator ServiceActivator { get; set; }

        /// <summary>
        /// Constructor can handle only error detail. If you want to set the other options, you can use object initializer. 
        /// </summary>
        /// <param name="isReturnExceptionStackTraceInErrorDetail">true, when method body throws exception send to client exception.ToString message. It is useful for debugging. Default is false.</param>
        public MagicOnionOptions(bool isReturnExceptionStackTraceInErrorDetail = false)
        {
            this.IsReturnExceptionStackTraceInErrorDetail = isReturnExceptionStackTraceInErrorDetail;
            this.FormatterResolver = MessagePackSerializer.DefaultResolver;
            this.MagicOnionLogger = new NullMagicOnionLogger();
            this.GlobalFilters = new List<MagicOnionServiceFilterDescriptor>();
            this.GlobalStreamingHubFilters = new List<StreamingHubFilterDescriptor>();
            this.DefaultGroupRepositoryFactory = new ImmutableArrayGroupRepositoryFactory();
            this.DisableEmbeddedService = false;
            this.EnableCurrentContext = false;

            this.ServiceLocator = DefaultServiceLocator.Instance;
            this.ServiceActivator = DefaultServiceActivator.Instance;
        }
    }

    /// <summary>
    /// Provides some services from MagicOnionOptions and ServiceLocator.
    /// </summary>
    internal class ServiceLocatorOptionAdapter : IServiceLocator
    {
        readonly MagicOnionOptions options;

        public ServiceLocatorOptionAdapter(MagicOnionOptions options)
        {
            this.options = options;
        }

        public T GetService<T>()
        {
            var t = typeof(T);
            var value = default(T);

            if (t == typeof(IFormatterResolver)) value = (T)options.FormatterResolver;
            else if (t == typeof(IMagicOnionLogger)) value = (T)options.MagicOnionLogger;
            else if (t == typeof(IGroupRepositoryFactory)) value = (T)options.DefaultGroupRepositoryFactory;
            else if (t == typeof(IServiceActivator)) value = (T)options.ServiceActivator;

            return value != null
                ? value
                : options.ServiceLocator.GetService<T>();
        }
    }
}