using System;
using System.Threading.Tasks;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Client
{
    public abstract class ResponseContext : IDisposable
    {
        static readonly Func<byte[], byte[]> DefaultMutator = xs => xs;

        public abstract Task<Metadata> ResponseHeadersAsync { get; }
        public abstract Status GetStatus();
        public abstract Metadata GetTrailers();
        public abstract void Dispose();
        public abstract Type ResponseType { get; }
        public Func<byte[], byte[]> ResponseMutator { get; private set; }

        public abstract Task<ResponseContext> WaitResponseAsync();

        public ResponseContext<T> As<T>()
        {
            return this as ResponseContext<T>;
        }

        public Task<T> GetResponseAs<T>()
        {
            var t = this as ResponseContext<T>;
            if (t == null) return Task.FromResult(default(T));

            return t.ResponseAsync;
        }

        public ResponseContext()
        {
            this.ResponseMutator = DefaultMutator;
        }

        public void SetResponseMutator(Func<byte[], byte[]> mutator)
        {
            this.ResponseMutator = mutator;
        }
    }

    public sealed class ResponseContext<T> : ResponseContext
    {
        readonly AsyncUnaryCall<byte[]> inner;
        readonly IFormatterResolver resolver;
        readonly bool isMock;
        bool deserialized;

        T responseObject; // cache value.

        // mock
        readonly Metadata trailers;
        readonly Metadata responseHeaders;
        readonly Status status;

        public ResponseContext(AsyncUnaryCall<byte[]> inner, IFormatterResolver resolver)
            : base()
        {
            this.isMock = false;
            this.inner = inner;
            this.resolver = resolver;
        }

        public ResponseContext(T responseObject)
            : this(responseObject, new Metadata(), new Metadata(), Status.DefaultSuccess)
        {
        }

        public ResponseContext(T responseObject, Metadata trailers, Metadata responseHeaders, Status status)
            : base()
        {
            this.isMock = true;
            this.responseObject = responseObject;
            this.trailers = trailers;
            this.responseHeaders = responseHeaders;
            this.status = status;
        }

        async Task<T> Deserialize()
        {
            if (deserialized)
            {
                return responseObject;
            }
            else
            {
                var bytes = await inner.ResponseAsync.ConfigureAwait(false);
                responseObject = LZ4MessagePackSerializer.Deserialize<T>(this.ResponseMutator(bytes), resolver);
                deserialized = true;
                return responseObject;
            }
        }

        public Task<T> ResponseAsync => !isMock ? Deserialize() : Task.FromResult(responseObject);

        public override async Task<ResponseContext> WaitResponseAsync()
        {
            await ResponseAsync;
            return this;
        }

        public override Type ResponseType => typeof(T);

        public override Task<Metadata> ResponseHeadersAsync => !isMock ? inner.ResponseHeadersAsync : Task.FromResult(responseHeaders);

        public override void Dispose()
        {
            if (!isMock)
            {
                inner.Dispose();
            }
        }

        public override Status GetStatus()
        {
            return !isMock ? inner.GetStatus() : status;
        }

        public override Metadata GetTrailers()
        {
            return !isMock ? inner.GetTrailers() : trailers;
        }

        public ResponseContext<T> WithNewResult(T result)
        {
            if (isMock)
            {
                return new ResponseContext<T>(result, trailers, responseHeaders, status);
            }
            else
            {
                var newContext = new ResponseContext<T>(inner, resolver);
                newContext.deserialized = true;
                newContext.responseObject = result;
                return newContext;
            }
        }
    }
}