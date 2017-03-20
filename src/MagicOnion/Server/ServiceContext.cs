using Grpc.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public class ServiceContext
    {
        Dictionary<string, object> items;

        /// <summary>Object storage per invoke.</summary>
        public IDictionary<string, object> Items
        {
            get
            {
                if (items == null) items = new Dictionary<string, object>();
                return items;
            }
        }

        public DateTime Timestamp { get; private set; }

        public Type ServiceType { get; private set; }

        public MethodInfo MethodInfo { get; private set; }

        /// <summary>Cached Attributes both service and method.</summary>
        public ILookup<Type, Attribute> AttributeLookup { get; private set; }

        public MethodType MethodType { get; private set; }

        /// <summary>Raw gRPC Context.</summary>
        public ServerCallContext CallContext { get; private set; }

        public IFormatterResolver FormatterResolver { get; private set; }

        // internal, used from there methods.
        internal byte[] Request { get; set; }
        internal IAsyncStreamReader<byte[]> RequestStream { get; set; }
        internal IAsyncStreamWriter<byte[]> ResponseStream { get; set; }
        internal byte[] Result { get; set; }
        internal IMagicOnionLogger MagicOnionLogger { get; private set; }

        public ServiceContext(Type serviceType, MethodInfo methodInfo, ILookup<Type, Attribute> attributeLookup, MethodType methodType, ServerCallContext context, IFormatterResolver resolver, IMagicOnionLogger logger)
        {
            this.ServiceType = serviceType;
            this.MethodInfo = methodInfo;
            this.AttributeLookup = attributeLookup;
            this.MethodType = methodType;
            this.CallContext = context;
            this.Timestamp = DateTime.UtcNow;
            this.FormatterResolver = resolver;
            this.MagicOnionLogger = logger;
        }

        /// <summary>
        /// modify request/response resolver in this context.
        /// </summary>
        public void ChangeFormatterResolver(IFormatterResolver resolver)
        {
            this.FormatterResolver = resolver;
        }
    }

    public class ClientStreamingContext<TRequest, TResponse> : IAsyncStreamReader<TRequest>
    {
        readonly ServiceContext context;
        readonly IAsyncStreamReader<byte[]> inner;
        readonly IMagicOnionLogger logger;
        static readonly byte[] emptyBytes = new byte[0];

        public ClientStreamingContext(ServiceContext context)
        {
            this.context = context;
            this.inner = context.RequestStream;
            this.logger = context.MagicOnionLogger;
        }

        public ServiceContext ServiceContext { get { return context; } }

        public TRequest Current { get; private set; }

        public async Task<bool> MoveNext(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await inner.MoveNext(cancellationToken))
            {
                var data = inner.Current;
                logger.ReadFromStream(context, data, typeof(TRequest), false);
                this.Current = LZ4MessagePackSerializer.Deserialize<TRequest>(inner.Current, context.FormatterResolver);
                return true;
            }
            else
            {
                logger.ReadFromStream(context, emptyBytes, typeof(Nil), true);
                return false;
            }
        }

        public void Dispose()
        {
            inner.Dispose();
        }

        public async Task ForEachAsync(Action<TRequest> action)
        {
            while (await MoveNext(CancellationToken.None)) // ClientResponseStream is not supported CancellationToken.
            {
                action(Current);
            }
        }

        public async Task ForEachAsync(Func<TRequest, Task> asyncAction)
        {
            while (await MoveNext(CancellationToken.None))
            {
                await asyncAction(Current);
            }
        }

        public ClientStreamingResult<TRequest, TResponse> Result(TResponse result)
        {
            var bytes = LZ4MessagePackSerializer.Serialize<TResponse>(result, context.FormatterResolver);
            context.Result = bytes;

            return default(ClientStreamingResult<TRequest, TResponse>); // dummy
        }

        public ClientStreamingResult<TRequest, TResponse> ReturnStatus(StatusCode statusCode, string detail)
        {
            context.CallContext.Status = new Status(statusCode, detail);

            return default(ClientStreamingResult<TRequest, TResponse>); // dummy
        }
    }

    public class ServerStreamingContext<TResponse> : IAsyncStreamWriter<TResponse>
    {
        readonly ServiceContext context;
        readonly IAsyncStreamWriter<byte[]> inner;
        readonly IMagicOnionLogger logger;

        public ServerStreamingContext(ServiceContext context)
        {
            this.context = context;
            this.inner = context.ResponseStream;
            this.logger = context.MagicOnionLogger;
        }

        public ServiceContext ServiceContext { get { return context; } }

        public WriteOptions WriteOptions
        {
            get
            {
                return inner.WriteOptions;
            }

            set
            {
                inner.WriteOptions = value;
            }
        }

        public Task WriteAsync(TResponse message)
        {
            var bytes = LZ4MessagePackSerializer.Serialize(message, context.FormatterResolver);
            logger.WriteToStream(context, bytes, typeof(TResponse));
            return inner.WriteAsync(bytes);
        }

        public ServerStreamingResult<TResponse> Result()
        {
            return default(ServerStreamingResult<TResponse>); // dummy
        }

        public ServerStreamingResult<TResponse> ReturnStatus(StatusCode statusCode, string detail)
        {
            context.CallContext.Status = new Status(statusCode, detail);

            return default(ServerStreamingResult<TResponse>); // dummy
        }
    }

    public class DuplexStreamingContext<TRequest, TResponse> : IAsyncStreamReader<TRequest>, IServerStreamWriter<TResponse>
    {
        readonly ServiceContext context;
        readonly IAsyncStreamReader<byte[]> innerReader;
        readonly IAsyncStreamWriter<byte[]> innerWriter;
        readonly IMagicOnionLogger logger;
        static readonly byte[] emptyBytes = new byte[0];

        public DuplexStreamingContext(ServiceContext context)
        {
            this.context = context;
            this.innerReader = context.RequestStream;
            this.innerWriter = context.ResponseStream;
            this.logger = context.MagicOnionLogger;
        }

        public ServiceContext ServiceContext { get { return context; } }

        /// <summary>IAsyncStreamReader Methods.</summary>
        public TRequest Current { get; private set; }

        /// <summary>IAsyncStreamReader Methods.</summary>
        public async Task<bool> MoveNext(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await innerReader.MoveNext(cancellationToken))
            {
                var data = innerReader.Current;
                logger.ReadFromStream(context, data, typeof(TRequest), false);
                this.Current = LZ4MessagePackSerializer.Deserialize<TRequest>(data, context.FormatterResolver);
                return true;
            }
            else
            {
                logger.ReadFromStream(context, emptyBytes, typeof(Nil), true);
                return false;
            }
        }

        /// <summary>IAsyncStreamReader Methods.</summary>
        public void Dispose()
        {
            innerReader.Dispose();
        }

        /// <summary>
        /// IServerStreamWriter Methods.
        /// </summary>
        public WriteOptions WriteOptions
        {
            get
            {
                return innerWriter.WriteOptions;
            }

            set
            {
                innerWriter.WriteOptions = value;
            }
        }

        /// <summary>
        /// IServerStreamWriter Methods.
        /// </summary>
        public Task WriteAsync(TResponse message)
        {
            var bytes = LZ4MessagePackSerializer.Serialize(message, context.FormatterResolver);
            logger.WriteToStream(context, bytes, typeof(TResponse));
            return innerWriter.WriteAsync(bytes);
        }

        public DuplexStreamingResult<TRequest, TResponse> Result()
        {
            return default(DuplexStreamingResult<TRequest, TResponse>); // dummy
        }

        public DuplexStreamingResult<TRequest, TResponse> ReturnStatus(StatusCode statusCode, string detail)
        {
            context.CallContext.Status = new Status(statusCode, detail);

            return default(DuplexStreamingResult<TRequest, TResponse>); // dummy
        }
    }
}