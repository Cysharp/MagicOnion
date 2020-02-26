using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.Server.Authentication
{
    /// <summary>
    /// Specifies that the class or method that this attribute is applied to does not require authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowAnonymousAttribute : MagicOnionFilterAttribute
    {
        public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            return next(context);
        }
    }
}
