using System;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Internal;

namespace MagicOnion.Client
{
    internal class ResponseContextRaw<T, TRaw> : ResponseContext<T>
    {
        readonly AsyncUnaryCall<TRaw> inner;

        readonly bool hasValue;
        readonly T value;

        readonly bool hasMetadataAndStatus;
        readonly Status status;
        readonly Metadata responseHeaders;
        readonly Metadata trailers;
        readonly Func<TRaw, T> fromRawResponseToResponse;

        public ResponseContextRaw(T value, Status status, Metadata responseHeaders, Metadata trailers)
            : this(null, hasValue: true, value, hasMetadataAndStatus: true, status, responseHeaders: responseHeaders, trailers: trailers, x => (T)(object)x)
        { }

        public ResponseContextRaw(AsyncUnaryCall<TRaw> inner, Func<TRaw, T> fromRawResponseToResponse)
            : this(inner, hasValue: false, default, hasMetadataAndStatus: false, default, default, default, fromRawResponseToResponse)
        { }

        public ResponseContextRaw(AsyncUnaryCall<TRaw> inner, bool hasValue, T value, bool hasMetadataAndStatus, Status status, Metadata responseHeaders, Metadata trailers, Func<TRaw, T> fromRawResponseToResponse)
        {
            if (!hasValue && inner == null) throw new ArgumentNullException(nameof(inner));
            if (hasMetadataAndStatus && responseHeaders == null) throw new ArgumentNullException(nameof(responseHeaders));
            if (hasMetadataAndStatus && trailers == null) throw new ArgumentNullException(nameof(trailers));
            if (fromRawResponseToResponse == null) throw new ArgumentNullException(nameof(fromRawResponseToResponse));

            this.inner = inner;

            this.hasValue = hasValue;
            this.value = value;

            this.hasMetadataAndStatus = hasMetadataAndStatus;
            this.status = status;
            this.responseHeaders = responseHeaders;
            this.trailers = trailers;
            this.fromRawResponseToResponse = fromRawResponseToResponse;
        }

        public override Type ResponseType => typeof(T);

        public override async Task<ResponseContext> WaitResponseAsync()
        {
            await ResponseAsync.ConfigureAwait(false);
            return this;
        }

        public override Status GetStatus()
            => hasMetadataAndStatus
                ? status
                : inner.GetStatus();
        
        public override Task<Metadata> ResponseHeadersAsync
            => hasMetadataAndStatus
                ? Task.FromResult(responseHeaders)
                : inner.ResponseHeadersAsync;
        public override  Metadata GetTrailers()
            => hasMetadataAndStatus
                ? trailers
                : inner.GetTrailers();

        public override Task<T> ResponseAsync
            => hasValue
                ? Task.FromResult(value)
                : FromRawResponseToResponseAsync();

        async Task<T> FromRawResponseToResponseAsync()
            => fromRawResponseToResponse(await inner.ResponseAsync.ConfigureAwait(false));
 
        public override void Dispose()
            => inner?.Dispose();

        public override ResponseContext<T> WithNewResult(T newValue)
            => new ResponseContextRaw<T, TRaw>(inner, hasValue: true, newValue, hasMetadataAndStatus, status, responseHeaders, trailers, fromRawResponseToResponse);
    }

    public abstract class ResponseContext<T> : ResponseContext, IResponseContext<T>
    {
        public static ResponseContext<T> Create<TRaw>(AsyncUnaryCall<TRaw> inner, Func<TRaw, T> fromRawResponseToResponse)
            => new ResponseContextRaw<T, TRaw>(inner, fromRawResponseToResponse);

        public static ResponseContext<T> Create(T value, Status status, Metadata responseHeaders, Metadata trailers)
            => new ResponseContextRaw<T, T>(null, hasValue: true, value, hasMetadataAndStatus: true, status, responseHeaders: responseHeaders, trailers: trailers, x => x);

        public abstract Task<T> ResponseAsync { get; }
        public abstract ResponseContext<T> WithNewResult(T newValue);
    }

    public abstract class ResponseContext : IResponseContext
    {
        public abstract Task<Metadata> ResponseHeadersAsync { get; }
        public abstract Status GetStatus();
        public abstract Metadata GetTrailers();
        public abstract void Dispose();
        public abstract Type ResponseType { get; }

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
    }
}
