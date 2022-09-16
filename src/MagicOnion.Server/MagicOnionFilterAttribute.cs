using MagicOnion.Server.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class MagicOnionFilterAttribute : Attribute, IMagicOnionServiceFilter
    {
        public int Order { get; set; } = int.MaxValue;

        /// <summary>
        /// This constructor used by MagicOnionEngine when register handler.
        /// </summary>
        public MagicOnionFilterAttribute()
        {
        }

        public abstract ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next);

        protected static void SetStatusCode(ServiceContext context, Grpc.Core.StatusCode statusCode, string detail)
        {
            context.CallContext.Status = new Grpc.Core.Status(statusCode, detail);
        }
    }
}