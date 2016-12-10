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

        // TODO:Result...
        protected IAsyncStreamReader<T> GetStreamReader<T>()
        {
            return null;
        }

        protected IAsyncStreamWriter<T> GetStreamWriter<T>()
        {
            return null;
        }


        protected UnaryResult<T> UnaryResult<T>(T result)
        {
            var marshaller = Context.UnaryMarshaller;
            if (marshaller == null) throw new Exception();

            var serializer = marshaller as Marshaller<T>;
            if (serializer == null) throw new Exception();

            var bytes = serializer.Serializer(result);
            Context.UnaryResult = bytes;

            return default(UnaryResult<T>); // dummy
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
}
