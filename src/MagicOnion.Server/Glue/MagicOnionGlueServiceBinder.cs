using System;
using System.Collections.Generic;
using System.Text;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;

namespace MagicOnion.Server.Glue
{
    internal class MagicOnionGlueServiceBinder<TService> : ServiceBinderBase
        where TService : class
    {
        private readonly ServiceMethodProviderContext<TService> _context;

        public MagicOnionGlueServiceBinder(ServiceMethodProviderContext<TService> context)
        {
            _context = context;
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse> handler)
        {
            _context.AddUnaryMethod(method, Array.Empty<object>(), (_, request, context) => handler(request, context));
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ClientStreamingServerMethod<TRequest, TResponse> handler)
        {
            _context.AddClientStreamingMethod(method, Array.Empty<object>(), (_, request, context) => handler(request, context));
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServerStreamingServerMethod<TRequest, TResponse> handler)
        {
            _context.AddServerStreamingMethod(method, Array.Empty<object>(), (_, request, stream, context) => handler(request, stream, context));
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, DuplexStreamingServerMethod<TRequest, TResponse> handler)
        {
            _context.AddDuplexStreamingMethod(method, Array.Empty<object>(), (_, request, stream, context) => handler(request, stream, context));
        }
    }
}
