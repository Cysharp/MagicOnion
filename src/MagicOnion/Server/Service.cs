using Grpc.Core;
using System;
using System.Threading;

namespace MagicOnion.Server
{
    public interface IStreamingService
    {
        ClientStreamingContext<TRequest, TResponse> GetClientStreamingContext<TRequest, TResponse>();
        ServerStreamingContext<TResponse> GetServerStreamingContext<TResponse>();
        DuplexStreamingContext<TRequest, TResponse> GetDuplexStreamingContext<TRequest, TResponse>();
    }

    public abstract class ServiceBase<TServiceInterface> : IService<TServiceInterface>, IStreamingService
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

        protected UnaryResult<TResponse> ReturnStatus<TResponse>(StatusCode statusCode, string detail)
        {
            Context.CallContext.Status = new Status(statusCode, detail);

            return default(UnaryResult<TResponse>); // dummy
        }

        [Ignore]
        public ClientStreamingContext<TRequest, TResponse> GetClientStreamingContext<TRequest, TResponse>()
        {
            return new ClientStreamingContext<TRequest, TResponse>(Context);
        }

        [Ignore]
        public ServerStreamingContext<TResponse> GetServerStreamingContext<TResponse>()
        {
            return new ServerStreamingContext<TResponse>(Context);
        }

        [Ignore]
        public DuplexStreamingContext<TRequest, TResponse> GetDuplexStreamingContext<TRequest, TResponse>()
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