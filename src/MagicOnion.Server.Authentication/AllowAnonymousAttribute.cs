using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.Server.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowAnonymousAttribute : MagicOnionFilterAttribute
    {
        public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            return next(context);
        }
    }
}
