using Grpc.Core;
using MagicOnion.HttpGateway;
using MagicOnion.HttpGateway.Swagger;
using MagicOnion.Server;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion
{
    public static class MagicOnionMiddlewareExtensions
    {
        public static IApplicationBuilder UseMagicOnionHttpGateway(this IApplicationBuilder app, IReadOnlyList<MethodHandler> handlers, Channel channel)
        {
            return app.UseMiddleware<MagicOnionHttpGatewayMiddleware>(handlers, channel);
        }

        public static IApplicationBuilder UseMagicOnionSwagger(this IApplicationBuilder app, IReadOnlyList<MethodHandler> handlers, SwaggerOptions options)
        {
            return app.UseMiddleware<MagicOnionSwaggerMiddleware>(handlers, options);
        }
    }
}