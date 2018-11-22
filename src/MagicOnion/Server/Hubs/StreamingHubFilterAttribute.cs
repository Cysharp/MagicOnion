using System;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class StreamingHubFilterAttribute : Attribute
    {
        int order = int.MaxValue;
        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        protected Func<StreamingHubContext, ValueTask> Next { get; private set; }

        /// <summary>
        /// This constructor used by MagicOnionEngine when register handler.
        /// </summary>
        public StreamingHubFilterAttribute(Func<StreamingHubContext, ValueTask> next)
        {
            this.Next = next;
        }

        public abstract ValueTask Invoke(StreamingHubContext context);
    }
}