using Microsoft.Extensions.Logging;
using System;
using System.Threading.Channels;

namespace MagicOnion.Server
{
    // Grpc's ResponseStream(IAsyncStreamWriter) does not allow multithread call.
    // IGroup is sometimes called from many caller(multithread) and invoke ResponseStream.Write
    // So requires queueing.

    internal class QueuedResponseWriter : IDisposable
    {
        ServiceContext serviceContext;
        Channel<byte[]> channel;

        public QueuedResponseWriter(ServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
            this.channel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = false
            });

            ConsumeQueueAsync();
        }

        public void Write(byte[] value)
        {
            channel.Writer.TryWrite(value);
        }

        async void ConsumeQueueAsync()
        {
            var reader = channel.Reader;
            var stream = serviceContext.ResponseStream!;
            do
            {
                while (reader.TryRead(out var item))
                {
                    if (serviceContext.IsDisconnected) break;
                    try
                    {
                        await stream.WriteAsync(item).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        MagicOnionServerInternalLogger.Current.LogError(ex, "error occurred on write to client.");
                    }
                }
                if (serviceContext.IsDisconnected) break;
            } while (await reader.WaitToReadAsync().ConfigureAwait(false));

        }

        public void Dispose()
        {
            channel.Writer.TryComplete();
        }
    }
}