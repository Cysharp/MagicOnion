using MagicOnion.Server;
using System;
using System.Threading.Tasks;

namespace MicroServer
{
    /// <summary>
    /// Application Exception Filter to set gRPC Status as app wants
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
                SetStatusCode(context, ConvertToGrpcStatus(ex), ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Your Exception -> gRPC Status Converter
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private Grpc.Core.StatusCode ConvertToGrpcStatus(Exception ex)
        {
            return ex switch
            {
                NotImplementedException => Grpc.Core.StatusCode.Unimplemented,
                _ => Grpc.Core.StatusCode.Internal,
            };
        }
    }
}