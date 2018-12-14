using System;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class MagicOnionFilterAttribute : Attribute
    {
        int order = int.MaxValue;
        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        protected Func<ServiceContext, ValueTask> Next { get; private set; }

        /// <summary>
        /// This constructor used by MagicOnionEngine when register handler.
        /// </summary>
        public MagicOnionFilterAttribute(Func<ServiceContext, ValueTask> next)
        {
            this.Next = next;
        }

        public abstract ValueTask Invoke(ServiceContext context);

        protected static void SetStatusCode(ServiceContext context, Grpc.Core.StatusCode statusCode, string detail)
        {
            context.CallContext.Status = new Grpc.Core.Status(statusCode, detail);
        }
    }
}