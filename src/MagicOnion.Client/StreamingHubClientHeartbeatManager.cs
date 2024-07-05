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
    /// <summary>
    /// Represents a client heartbeat received event.
    /// </summary>
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

    /// <summary>
    /// Represents a server heartbeat received event.
    /// </summary>
    public readonly struct ServerHeartbeatEvent
    {
        /// <summary>
        /// Gets the server time at when the heartbeat was sent.
        /// </summary>
        public DateTimeOffset ServerTime { get; }

        /// <summary>
        /// Gets the metadata data. The data is only available during event processing.
        /// </summary>
        public ReadOnlyMemory<byte> Metadata { get; }

        public ServerHeartbeatEvent(long serverTimeUnixMs, ReadOnlyMemory<byte> metadata)
        {
            ServerTime = DateTimeOffset.FromUnixTimeMilliseconds(serverTimeUnixMs);
            Metadata = metadata;
        }
    }

    internal class StreamingHubClientHeartbeatManager : IDisposable
    {
        readonly CancellationTokenSource timeoutTokenSource;
        readonly CancellationTokenSource shutdownTokenSource;
        readonly TimeSpan heartbeatInterval;
        readonly TimeSpan timeoutPeriod;
        readonly Action<ServerHeartbeatEvent>? onServerHeartbeatReceived;
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
            Action<ServerHeartbeatEvent>? onServerHeartbeatReceived,
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
            var (sentSequence, clientSentAt) = reader.ReadClientHeartbeatResponse();

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
            var elapsed = now.ToUnixTimeMilliseconds() - clientSentAt;

            clientHeartbeatReceivedAction?.Invoke(new ClientHeartbeatEvent(elapsed));
            StreamingHubPayloadPool.Shared.Return(payload);
        };

        SendOrPostCallback ProcessServerHeartbeatCore(Action<ServerHeartbeatEvent>? serverHeartbeatReceivedAction) => (state) =>
        {
            var payload = (StreamingHubPayload)state!;
            var reader = new StreamingHubClientMessageReader(payload.Memory);
            _ = reader.ReadMessageType();
            var (serverSentSequence, serverSentAt, metadata) = reader.ReadServerHeartbeat();

            serverHeartbeatReceivedAction?.Invoke(new ServerHeartbeatEvent(serverSentAt, metadata));

            // Writes a ServerHeartbeatResponse to the writer queue.
            _ = writer.TryWrite(BuildServerHeartbeatMessage(serverSentSequence, serverSentAt));

            StreamingHubPayloadPool.Shared.Return(payload);
        };

        StreamingHubPayload BuildServerHeartbeatMessage(short serverSequence, long serverSentAt)
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
            StreamingHubMessageWriter.WriteServerHeartbeatMessageResponse(buffer, serverSequence, serverSentAt);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        StreamingHubPayload BuildClientHeartbeatMessage(short clientSequence)
        {
            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();

            var now =
#if NET8_0_OR_GREATER
                timeProvider.GetUtcNow();
#else
                DateTimeOffset.UtcNow;
#endif

            StreamingHubMessageWriter.WriteClientHeartbeatMessage(buffer, clientSequence, now);
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
