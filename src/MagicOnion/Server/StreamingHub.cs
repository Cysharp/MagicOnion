using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    // used for MagicOnionEngine assembly scan for boostup analyze speed.
    public interface IStreamingHubMarker
    {
    }

    public interface IStreamingHub<TReceiver> : IStreamingHubMarker
    {
        void Subscribe(TReceiver receiver);

        // DisconnectReason WaitDisconnectAsync ???
        // void OnDisconnect(Action action);
    }


    public class StreamingHubContext
    {

    }






    public abstract class StreamingHubBase<TReceiver> : IStreamingHub<TReceiver>
    {
        static protected readonly ValueTask CompletedTask = new ValueTask();

        public StreamingHubContext Context { get; set; }

        /// <summary>
        /// Get Grpc Logger.
        /// </summary>
        protected Grpc.Core.Logging.ILogger Logger => GrpcEnvironment.Logger;

        protected virtual ValueTask OnConnecting()
        {
            return CompletedTask;
        }

        protected virtual ValueTask OnCompleted()
        {
            return CompletedTask;
        }

        public async Task Connect()
        {
            //var streaming = new DuplexStreamingContext<byte[], byte[]>(Context);
            //var handler = new StreamingHubHandler(streaming);

            //await OnConnected();
            //try
            //{
            //    await handler.HandleAsync();
            //}
            //catch
            //{
            //}
            //await OnCompleted();
        }


        // Interface methods for Client

        void IStreamingHub<TReceiver>.Subscribe(TReceiver receiver)
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }
    }
}
