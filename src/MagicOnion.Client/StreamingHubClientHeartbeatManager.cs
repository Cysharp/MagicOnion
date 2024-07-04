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

        SendOrPostCallback? processServerHeartbeatCoreCache;
        SendOrPostCallback? proecssClientHeartbeatResponseCoreCache;
        Task? heartbeatLoopTask;
        short sequence;
        bool isTimeoutTimerRunning;

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
                await Task.Delay(heartbeatInterval
#if NET8_0_OR_GREATER
                    , timeProvider
#endif
                    , shutdownTokenSource.Token).ConfigureAwait(false);

                shutdownTokenSource.Token.ThrowIfCancellationRequested();

                // Writes a ClientHeartbeat to the writer queue.
                _ = writer.TryWrite(BuildClientHeartbeatMessage(sequence));

                if (!isTimeoutTimerRunning)
                {
                    // Start/Restart the timeout cancellation timer. 
                    timeoutTokenSource.CancelAfter(timeoutPeriod);
                    isTimeoutTimerRunning = true;
                }

                sequence++;
            }
        }

        public void ProcessClientHeartbeatResponse(StreamingHubPayload payload)
        {
            if (shutdownTokenSource.IsCancellationRequested) return;

            proecssClientHeartbeatResponseCoreCache ??= ProcessClientHeartbeatResponseCore(onClientHeartbeatResponseReceived);

            if (synchronizationContext is null)
            {
                proecssClientHeartbeatResponseCoreCache(payload);
            }
            else
            {
                synchronizationContext.Post(proecssClientHeartbeatResponseCoreCache, payload);
            }
        }

        public void ProcessServerHeartbeat(StreamingHubPayload payload)
        {
            if (shutdownTokenSource.IsCancellationRequested) return;

            processServerHeartbeatCoreCache ??= ProcessServerHeartbeatCore(onServerHeartbeatReceived);

            if (synchronizationContext is null)
            {
                processServerHeartbeatCoreCache(payload);
            }
            else
            {
                synchronizationContext.Post(processServerHeartbeatCoreCache, payload);
            }
        }

        SendOrPostCallback ProcessClientHeartbeatResponseCore(Action<ClientHeartbeatEvent>? clientHeartbeatReceivedAction) => (state) =>
        {
            var payload = (StreamingHubPayload)state!;
            var reader = new StreamingHubClientMessageReader(payload.Memory);
            _ = reader.ReadMessageType();
            var (sentSequence, sentAt) = reader.ReadClientHeartbeatResponse();

            if (sentSequence == (sequence - 1)/* NOTE: Sequence already 1 advanced.*/)
            {
                // Cancel the running timeout cancellation timer.
                timeoutTokenSource.CancelAfter(Timeout.InfiniteTimeSpan);
                isTimeoutTimerRunning = false;
            }

            var now =
#if NET8_0_OR_GREATER
                timeProvider.GetUtcNow();
#else
                DateTimeOffset.UtcNow;
#endif
            var elapsed = now.ToUnixTimeMilliseconds() - sentAt;

            clientHeartbeatReceivedAction?.Invoke(new ClientHeartbeatEvent(elapsed));
            StreamingHubPayloadPool.Shared.Return(payload);
        };

        SendOrPostCallback ProcessServerHeartbeatCore(Action<ReadOnlyMemory<byte>>? serverHeartbeatReceivedAction) => (state) =>
        {
            var payload = (StreamingHubPayload)state!;
            var reader = new StreamingHubClientMessageReader(payload.Memory);
            _ = reader.ReadMessageType();
            var (serverSentSequence, metadata) = reader.ReadServerHeartbeat();

            serverHeartbeatReceivedAction?.Invoke(metadata);

            // Writes a ServerHeartbeatResponse to the writer queue.
            _ = writer.TryWrite(BuildServerHeartbeatMessage(serverSentSequence));

            StreamingHubPayloadPool.Shared.Return(payload);
        };

        StreamingHubPayload BuildServerHeartbeatMessage(short serverSequence)
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteServerHeartbeatMessageResponse(buffer, serverSequence);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        StreamingHubPayload BuildClientHeartbeatMessage(short clientSequence)
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteClientHeartbeatMessageHeader(buffer, clientSequence);

            var now =
#if NET8_0_OR_GREATER
                timeProvider.GetUtcNow();
#else
                DateTimeOffset.UtcNow;
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
