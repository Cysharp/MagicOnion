using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public abstract class ServiceBase<TServiceInterface> : IService<TServiceInterface>
    {
        public ServiceContext Context { get; set; }

        /// <summary>
        /// Get Grpc Logger.
        /// </summary>
        protected Grpc.Core.Logging.ILogger Logger => GrpcEnvironment.Logger;

        public ServiceBase()
        {

        }

        internal ServiceBase(ServiceContext context)
        {
            this.Context = context;
        }

        // Unary

        protected UnaryResult<TResponse> UnaryResult<TResponse>(TResponse result)
        {
            var marshaller = Context.ResponseMarshaller;
            if (marshaller == null) throw new Exception();

            var serializer = marshaller as Marshaller<TResponse>;
            if (serializer == null) throw new Exception();

            var bytes = serializer.Serializer(result);
            Context.Result = bytes;

            return default(UnaryResult<TResponse>); // dummy
        }

        // ClientStreaming

        protected ClientStreamingContext<TRequest, TResponse> GetClientStreamingContext<TRequest, TResponse>()
        {
            return new ClientStreamingContext<TRequest, TResponse>(Context);
        }

        protected IAsyncStreamWriter<T> GetStreamWriter<T>()
        {
            return null;
        }


        TServiceInterface IService<TServiceInterface>.WithOption(CallOptions option)
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }

        TServiceInterface IService<TServiceInterface>.WithHeaders(Metadata headers)
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }

        TServiceInterface IService<TServiceInterface>.WithDeadline(DateTime deadline)
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }

        TServiceInterface IService<TServiceInterface>.WithCancellationToken(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }

        TServiceInterface IService<TServiceInterface>.WithHost(string host)
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }
    }

    public class ClientStreamingContext<TRequest, TResponse> : IAsyncStreamReader<TRequest>
    {
        readonly ServiceContext context;
        readonly IAsyncStreamReader<byte[]> inner;
        readonly Marshaller<TRequest> marshaller;

        public ClientStreamingContext(ServiceContext context)
        {
            this.context = context;
            this.marshaller = (Marshaller<TRequest>)context.RequestMarshaller;
            this.inner = context.RequestStream;
        }

        public TRequest Current { get; private set; }

        public async Task<bool> MoveNext(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await inner.MoveNext(cancellationToken))
            {
                this.Current = marshaller.Deserializer(inner.Current);
                return true;
            }
            else
            {
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
            var marshaller = context.ResponseMarshaller;
            if (marshaller == null) throw new Exception();

            var serializer = marshaller as Marshaller<TResponse>;
            if (serializer == null) throw new Exception();

            var bytes = serializer.Serializer(result);
            context.Result = bytes;

            return default(ClientStreamingResult<TRequest, TResponse>); // dummy
        }
    }
}