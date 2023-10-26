using System;
using System.Collections.Generic;
using Grpc.Core;

namespace MagicOnion.Client
{
    public class RequestContext<T> : RequestContext
    {
        public T Request { get; }
        public override Type RequestType => typeof(T);

        public RequestContext(T request, MagicOnionClientBase client, string methodPath, CallOptions callOptions, Type responseType, IReadOnlyList<IClientFilter> filters, Func<RequestContext, ResponseContext> requestMethod)
            : base(client, methodPath, callOptions, responseType, filters, requestMethod)
        {
            this.Request = request;
        }
    }

    public abstract class RequestContext
    {
        Dictionary<string, object>? items;

        public string MethodPath { get; }
        public CallOptions CallOptions { get; }
        public Type ResponseType { get; }
        public abstract Type RequestType { get; }

        public IDictionary<string, object> Items
            => items ?? (items = new Dictionary<string, object>());

        // internal use to avoid lambda capture.
        internal MagicOnionClientBase Client { get; }
        internal IReadOnlyList<IClientFilter> Filters { get; }
        internal Func<RequestContext, ResponseContext> RequestMethod { get; }

        internal RequestContext(MagicOnionClientBase client, string methodPath, CallOptions callOptions, Type responseType, IReadOnlyList<IClientFilter> filters, Func<RequestContext, ResponseContext> requestMethod)
        {
            this.Client = client;
            this.MethodPath = methodPath;
            this.CallOptions = callOptions;
            this.ResponseType = responseType;
            this.Filters = filters;
            this.RequestMethod = requestMethod;
        }
    }
}
