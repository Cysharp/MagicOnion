using System;
using System.Collections.Generic;
using Grpc.Core;

namespace MagicOnion.Client
{
    public abstract class RequestContext
    {
        static readonly Func<byte[], byte[]> DefaultMutator = xs => xs;

        public string MethodPath { get; }
        public CallOptions CallOptions { get; }
        public Type ResponseType { get; }
        public abstract Type RequestType { get; }
        public Func<byte[], byte[]> RequestMutator { get; private set; }
        public Func<byte[], byte[]> ResponseMutator { get; private set; }

        Dictionary<string, object> items;
        public IDictionary<string, object> Items
        {
            get
            {
                if (items == null)
                {
                    items = new Dictionary<string, object>();
                }
                return items;
            }
        }

        // internal use to avoid lambda capture.
        internal MagicOnionClientBase Client { get; }
        internal IClientFilter[] Filters { get; }
        internal Func<RequestContext, ResponseContext> RequestMethod { get; }

        internal RequestContext(MagicOnionClientBase client, string methodPath, CallOptions callOptions, Type responseType, IClientFilter[] filters, Func<RequestContext, ResponseContext> requestMethod)
        {
            this.Client = client;
            this.MethodPath = methodPath;
            this.CallOptions = callOptions;
            this.ResponseType = responseType;
            this.Filters = filters;
            this.RequestMethod = requestMethod;
            this.RequestMutator = DefaultMutator;
            this.ResponseMutator = DefaultMutator;
        }

        public void SetRequestMutator(Func<byte[], byte[]> mutator)
        {
            this.RequestMutator = mutator;
        }

        public void SetResponseMutator(Func<byte[], byte[]> mutator)
        {
            this.ResponseMutator = mutator;
        }
    }

    public class RequestContext<T> : RequestContext
    {
        public T Request { get; }
        public override Type RequestType => typeof(T);

        public RequestContext(T request, MagicOnionClientBase client, string methodPath, CallOptions callOptions, Type responseType, IClientFilter[] filters, Func<RequestContext, ResponseContext> requestMethod)
            : base(client, methodPath, callOptions, responseType, filters, requestMethod)
        {
            this.Request = request;
        }
    }
}