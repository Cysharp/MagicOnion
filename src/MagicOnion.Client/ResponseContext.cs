using System;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Internal;

namespace MagicOnion.Client
{
    public class ResponseContext<T> : ResponseContext, IResponseContext<T>
    {
        readonly IDisposable inner; // AsyncUnaryCall<T> or AsyncUnaryCall<Box<T>>

        readonly bool hasValue;
        readonly T value;

        readonly bool hasMetadataAndStatus;
        readonly Status status;
        readonly Metadata responseHeaders;
        readonly Metadata trailers;

        public static ResponseContext Create<TRaw>(AsyncUnaryCall<TRaw> inner)
        {
            return (typeof(TRaw) == typeof(Box<T>))
                ? new ResponseContext<T>((AsyncUnaryCall<Box<T>>)(object)inner)
                : new ResponseContext<T>((AsyncUnaryCall<T>)(object)inner);
        }

        public ResponseContext(AsyncUnaryCall<T> inner)
            : this(inner, hasValue: false, default, hasMetadataAndStatus: false, default, default, default)
        { }
        public ResponseContext(AsyncUnaryCall<Box<T>> inner)
            : this(inner, hasValue: false, default, hasMetadataAndStatus: false, default, default, default)
        { }
        public ResponseContext(T value, AsyncUnaryCall<T> inner)
            : this(inner, hasValue: true, value, hasMetadataAndStatus: false, default, default, default)
        { }
        public ResponseContext(T value, AsyncUnaryCall<Box<T>> inner)
            : this(inner, hasValue: true, value, hasMetadataAndStatus: false, default, default, default)
        { }
        public ResponseContext(T value, Status status, Metadata responseHeaders, Metadata trailers)
            : this(default, hasValue: true, value, hasMetadataAndStatus: true, status, responseHeaders, trailers)
        { }

        private ResponseContext(IDisposable inner, bool hasValue, T value, bool hasMetadataAndStatus, Status status, Metadata responseHeaders, Metadata trailers)
        {
            if (!hasValue && inner == null) throw new ArgumentNullException(nameof(inner));
            if (hasMetadataAndStatus && responseHeaders == null) throw new ArgumentNullException(nameof(responseHeaders));
            if (hasMetadataAndStatus && trailers == null) throw new ArgumentNullException(nameof(trailers));

            this.inner = inner;

            this.hasValue = hasValue;
            this.value = value;

            this.hasMetadataAndStatus = hasMetadataAndStatus;
            this.status = status;
            this.responseHeaders = responseHeaders;
            this.trailers = trailers;
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
                : (inner is AsyncUnaryCall<Box<T>> boxed)
                    ? boxed.GetStatus()
                    : ((AsyncUnaryCall<T>)inner).GetStatus();
        
        public override Task<Metadata> ResponseHeadersAsync
            => hasMetadataAndStatus
                ? Task.FromResult(responseHeaders)
                : (inner is AsyncUnaryCall<Box<T>> boxed)
                    ? boxed.ResponseHeadersAsync
                    : ((AsyncUnaryCall<T>)inner).ResponseHeadersAsync;
        public override  Metadata GetTrailers()
            => hasMetadataAndStatus
                ? trailers
                : (inner is AsyncUnaryCall<Box<T>> boxed)
                    ? boxed.GetTrailers()
                    : ((AsyncUnaryCall<T>)inner).GetTrailers();

        public Task<T> ResponseAsync
            => hasValue
                ? Task.FromResult(value)
                : (inner is AsyncUnaryCall<Box<T>> boxed)
                    ? UnboxResponseAsync(boxed)
                    : ((AsyncUnaryCall<T>)inner).ResponseAsync;

        private static async Task<T> UnboxResponseAsync(AsyncUnaryCall<Box<T>> boxed)
            => (await boxed.ResponseAsync.ConfigureAwait(false)).Value;

        public override void Dispose()
            => inner?.Dispose();

        public ResponseContext<T> WithNewResult(T newValue)
            => new ResponseContext<T>(inner, hasValue: true, newValue, hasMetadataAndStatus, status, responseHeaders, trailers);
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