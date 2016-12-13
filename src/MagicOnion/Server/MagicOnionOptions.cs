using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter.Formatters;

namespace MagicOnion.Server
{
    public class MagicOnionOptions
    {
        /// <summary>
        /// ZeroFormatter resolver. Default is DefaultResolver.
        /// </summary>
        public Type ZeroFormatterTypeResolverType { get; set; }

        /// <summary>
        /// If true, MagicOnion handles exception ownself and send to message. If false, propagate to gRPC engine. Default is false.
        /// </summary>
        public bool IsReturnExceptionStackTraceInErrorDetail { get; set; }

        /// <summary>
        /// Set the diagnostics info logger.
        /// </summary>
        public IMagicOnionLogger MagicOnionLogger { get; set; }

        /// <summary>
        /// Global MagicOnion filters.
        /// </summary>
        public MagicOnionFilterAttribute[] GlobalFilters { get; set; }

        /// <summary>
        /// Constructor can handle only error detail. If you want to set the other options, you can use object initializer. 
        /// </summary>
        /// <param name="isReturnExceptionStackTraceInErrorDetail">true, when method body throws exception send to client exception.ToString message. It is useful for debugging. Default is false.</param>
        public MagicOnionOptions(bool isReturnExceptionStackTraceInErrorDetail = false)
        {
            this.IsReturnExceptionStackTraceInErrorDetail = isReturnExceptionStackTraceInErrorDetail;
            this.ZeroFormatterTypeResolverType = typeof(ZeroFormatter.Formatters.DefaultResolver);
            this.MagicOnionLogger = new NullMagicOnionLogger();
            this.GlobalFilters = new MagicOnionFilterAttribute[0];
        }
    }
}