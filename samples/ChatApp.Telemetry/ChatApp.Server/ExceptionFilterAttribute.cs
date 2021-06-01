using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagicOnion.Server.OpenTelemetry
{
    /// <summary>
    /// Collect OpenTelemetry Tracer with Unary filter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ExceptionFilterFactoryAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public int Order { get; set; }

        MagicOnionFilterAttribute IMagicOnionFilterFactory<MagicOnionFilterAttribute>.CreateInstance(IServiceProvider serviceProvider)
        {
            return new ExceptionFilterAttribute();
        }
    }

    internal class ExceptionFilterAttribute : MagicOnionFilterAttribute
    {
        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case NotImplementedException:
                        SetStatusCode(context, Grpc.Core.StatusCode.Unimplemented, ex.Message);
                        break;
                    default:
                        SetStatusCode(context, Grpc.Core.StatusCode.Internal, ex.Message);
                        break;
                }
                throw;
            }
        }
    }
}