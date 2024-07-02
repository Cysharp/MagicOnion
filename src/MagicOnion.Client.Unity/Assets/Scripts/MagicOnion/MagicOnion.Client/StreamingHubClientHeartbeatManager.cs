using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using MagicOnion.Internal;
using MagicOnion.Internal.Buffers;
using MessagePack;

namespace MagicOnion.Client
{
    public readonly struct ClientHeartbeatEvent
    {
        /// <summary>
        /// Gets the round trip time (RTT) between client and server.
        /// </summary>
        public TimeSpan RoundTripTime { get; }

        public ClientHeartbeatEvent(long roundTripTimeMs)
        {
            RoundTripTime = TimeSpan.FromMilliseconds(roundTripTimeMs);
        }
    }

    internal class StreamingHubClientHeartbeatManager : IDisposable
    {
        readonly CancellationTokenSource timeoutTokenSource;
        readonly CancellationTokenSource shutdownTokenSource;
        readonly TimeSpan heartbeatInterval;
        readonly TimeSpan timeoutPeriod;
        readonly Action<ReadOnlyMemory<byte>>? onServerHeartbeatReceived;
        readonly Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived;
        readonly SynchronizationContext? synchronizationContext;
        readonly ChannelWriter<StreamingHubPayload> writer;
#if NET8_0_OR_GREATER
        readonly TimeProvider timeProvider;
#endif

        SendOrPostCallback? serverHeartbeatCallbackCache;
        SendOrPostCallback? clientHeartbeatResponseCallbackCache;
        Task? heartbeatLoopTask;

        public CancellationToken TimeoutToken => timeoutTokenSource.Token;

        public StreamingHubClientHeartbeatManager(
            ChannelWriter<StreamingHubPayload> writer,
            TimeSpan heartbeatInterval,
            TimeSpan timeoutPeriod,
            Action<ReadOnlyMemory<byte>>? onServerHeartbeatReceived,
            Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived,
            SynchronizationContext? synchronizationContext,
            CancellationToken shutdownToken
#if NET8_0_OR_GREATER
            , TimeProvider timeProvider
#endif
        )
        {
            this.timeoutTokenSource = new(
#if NET8_0_OR_GREATER
                Timeout.InfiniteTimeSpan, timeProvider
#endif
            );
            this.writer = writer;
            this.heartbeatInterval = heartbeatInterval;
            this.timeoutPeriod = timeoutPeriod;
            this.onServerHeartbeatReceived = onServerHeartbeatReceived;
            this.onClientHeartbeatResponseReceived = onClientHeartbeatResponseReceived;
            this.synchronizationContext = synchronizationContext;
            this.shutdownTokenSource = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken, timeoutTokenSource.Token);
#if NET8_0_OR_GREATER
            this.timeProvider = timeProvider;
#endif
        }

        public void StartClientHeartbeatLoop()
        {
            heartbeatLoopTask = RunClientHeartbeatLoopAsync();
        }

        async Task RunClientHeartbeatLoopAsync()
        {
            while (!shutdownTokenSource.IsCancellationRequested)
            {
                Debug.WriteLine("Wait until sending time...");
                await Task.Delay(heartbeatInterval
#if NET8_0_OR_GREATER
                    , timeProvider
#endif
                    , shutdownTokenSource.Token).ConfigureAwait(false);

                shutdownTokenSource.Token.ThrowIfCancellationRequested();

                // Writes a ClientHeartbeat to the writer queue.
                _ = writer.TryWrite(BuildClientHeartbeatMessage());
                Debug.WriteLine("Wrote ClientHeartbeat message.");

                // Start/Restart the timeout cancellation timer. 
                timeoutTokenSource.CancelAfter(timeoutPeriod);
            }
        }

        public void ProcessClientHeartbeatResponse(StreamingHubPayload payload)
        {
            if (shutdownTokenSource.IsCancellationRequested) return;

            // Cancel the running timeout cancellation timer.
            timeoutTokenSource.CancelAfter(Timeout.InfiniteTimeSpan);

            if (onClientHeartbeatResponseReceived is { } heartbeatReceived)
            {
                clientHeartbeatResponseCallbackCache ??= CreateClientHeartbeatResponseCallback(heartbeatReceived);

                if (synchronizationContext is null)
                {
                    clientHeartbeatResponseCallbackCache(payload);
                }
                else
                {
                    synchronizationContext.Post(clientHeartbeatResponseCallbackCache, payload);
                }
            }
        }

        public void ProcessServerHeartbeat(StreamingHubPayload payload)
        {
            if (shutdownTokenSource.IsCancellationRequested) return;

            if (onServerHeartbeatReceived is { } heartbeatReceived)
            {
                serverHeartbeatCallbackCache ??= CreateServerHeartbeatCallback(heartbeatReceived);

                if (synchronizationContext is null)
                {
                    serverHeartbeatCallbackCache(payload);
                }
                else
                {
                    synchronizationContext.Post(serverHeartbeatCallbackCache, payload);
                }
            }

            // Writes a ServerHeartbeatResponse to the writer queue.
            _ = writer.TryWrite(BuildServerHeartbeatMessage());
        }

        SendOrPostCallback CreateClientHeartbeatResponseCallback(Action<ClientHeartbeatEvent> heartbeatReceivedAction) => (state) =>
        {
            var p = (StreamingHubPayload)state!;

            var reader = new StreamingHubClientMessageReader(p.Memory);
            _ = reader.ReadMessageType();

#if NET8_0_OR_GREATER
            var now = timeProvider.GetUtcNow();
#else
            var now = DateTimeOffset.UtcNow;
#endif
            var sentAt = reader.ReadClientHeartbeatResponse();
            var elapsed = now.ToUnixTimeMilliseconds() - sentAt;

            heartbeatReceivedAction(new ClientHeartbeatEvent(elapsed));
            StreamingHubPayloadPool.Shared.Return(p);
        };

        SendOrPostCallback CreateServerHeartbeatCallback(Action<ReadOnlyMemory<byte>> heartbeatReceivedAction) => (state) =>
        {
            var p = (StreamingHubPayload)state!;
            var remain = p.Memory.Slice(5); // header
            heartbeatReceivedAction(remain);
            StreamingHubPayloadPool.Shared.Return(p);
        };

        StreamingHubPayload BuildServerHeartbeatMessage()
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteServerHeartbeatMessageResponse(buffer);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        StreamingHubPayload BuildClientHeartbeatMessage()
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteClientHeartbeatMessageHeader(buffer);

#if NET8_0_OR_GREATER
            var now = timeProvider.GetUtcNow();
#else
            var now = DateTimeOffset.UtcNow;
#endif

            // Extra: [SentAt(long)]
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(1);
            writer.Write(now.ToUnixTimeMilliseconds());
            writer.Flush();
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        public void Dispose()
        {
            shutdownTokenSource.Cancel();
            shutdownTokenSource.Dispose();
            timeoutTokenSource.Dispose();
        }
    }
}
