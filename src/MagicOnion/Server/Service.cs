using Grpc.Core;
using System;
using System.Threading;

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

        // Helpers

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

        protected ClientStreamingContext<TRequest, TResponse> GetClientStreamingContext<TRequest, TResponse>()
        {
            return new ClientStreamingContext<TRequest, TResponse>(Context);
        }

        protected ServerStreamingContext<TResponse> GetServerStreamingContext<TResponse>()
        {
            return new ServerStreamingContext<TResponse>(Context);
        }

        protected DuplexStreamingContext<TRequest, TResponse> GetDuplexStreamingContext<TRequest, TResponse>()
        {
            return new DuplexStreamingContext<TRequest, TResponse>(Context);
        }

        // Interface methods for Client

        TServiceInterface IService<TServiceInterface>.WithOptions(CallOptions option)
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
}