using System;
using System.Threading.Tasks;
using MagicOnion.Server.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Hubs
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class StreamingHubFilterAttribute : Attribute, IStreamingHubFilter, IMagicOnionFilterFactory<IStreamingHubFilter>
    {
        public int Order { get; set; } = int.MaxValue;

        /// <summary>
        /// This constructor used by MagicOnionEngine when register handler.
        /// </summary>
        public StreamingHubFilterAttribute()
        {
        }

        public abstract ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next);

        IStreamingHubFilter IMagicOnionFilterFactory<IStreamingHubFilter>.CreateInstance(IServiceProvider serviceLocator)
        {
            return (IStreamingHubFilter)ActivatorUtilities.CreateInstance(serviceLocator, this.GetType());
        }
    }
}