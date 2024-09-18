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

    internal class StreamingHubClientHeartbeatManager : IAsyncDisposable
    {
        readonly object gate = new();
        readonly CancellationTokenSource timeoutTokenSource;
        readonly CancellationTokenSource shutdownTokenSource;
        readonly TimeSpan heartbeatInterval;
        readonly TimeSpan timeoutPeriod;
        readonly Action<ServerHeartbeatEvent>? onServerHeartbeatReceived;
        readonly Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived;
        readonly SynchronizationContext? synchronizationContext;
        readonly ChannelWriter<StreamingHubPayload> writer;
#if NON_UNITY
        readonly TimeProvider timeProvider;
#endif

        SendOrPostCallback? processServerHeartbeatCoreCache;
        SendOrPostCallback? processClientHeartbeatResponseCoreCache;
        Task? heartbeatLoopTask;
        short sequence;
        bool isTimeoutTimerRunning;
        bool disposed;

        public CancellationToken TimeoutToken => timeoutTokenSource.Token;

        bool IsDisposed => Volatile.Read(ref disposed);

        public StreamingHubClientHeartbeatManager(
            ChannelWriter<StreamingHubPayload> writer,
            TimeSpan heartbeatInterval,
            TimeSpan timeoutPeriod,
            Action<ServerHeartbeatEvent>? onServerHeartbeatReceived,
            Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived,
            SynchronizationContext? synchronizationContext,
            CancellationToken shutdownToken
#if NON_UNITY
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
#if NON_UNITY
            this.timeProvider = timeProvider;
#endif
        }

        public void StartClientHeartbeatLoop()
        {
            heartbeatLoopTask = RunClientHeartbeatLoopAsync();
        }

        async Task RunClientHeartbeatLoopAsync()
        {
            while (!IsDisposed)
            {
                await Task.Delay(heartbeatInterval
#if NET8_0_OR_GREATER
                    , timeProvider
#endif
                    , shutdownTokenSource.Token).ConfigureAwait(false);

                SendClientHeartbeat();
            }
        }

        void SendClientHeartbeat()
        {
            lock (gate)
            {
                if (IsDisposed) return;

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
            if (IsDisposed) return;

            processClientHeartbeatResponseCoreCache ??= ProcessClientHeartbeatResponseCore(onClientHeartbeatResponseReceived);

            if (synchronizationContext is null)
            {
                processClientHeartbeatResponseCoreCache(payload);
            }
            else
            {
                synchronizationContext.Post(processClientHeartbeatResponseCoreCache, payload);
            }
        }

        public void ProcessServerHeartbeat(StreamingHubPayload payload)
        {
            if (IsDisposed) return;

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
            lock (gate)
            {
                if (IsDisposed) return;

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
#if NON_UNITY
                    timeProvider.GetUtcNow();
#else
                    DateTimeOffset.UtcNow;
#endif
                var elapsed = now.ToUnixTimeMilliseconds() - clientSentAt;

                clientHeartbeatReceivedAction?.Invoke(new ClientHeartbeatEvent(elapsed));
                StreamingHubPayloadPool.Shared.Return(payload);
            }
        };

        SendOrPostCallback ProcessServerHeartbeatCore(Action<ServerHeartbeatEvent>? serverHeartbeatReceivedAction) => (state) =>
        {
            lock (gate)
            {
                if (IsDisposed) return;

                var payload = (StreamingHubPayload)state!;
                var reader = new StreamingHubClientMessageReader(payload.Memory);
                _ = reader.ReadMessageType();
                var (serverSentSequence, serverSentAt, metadata) = reader.ReadServerHeartbeat();

                serverHeartbeatReceivedAction?.Invoke(new ServerHeartbeatEvent(serverSentAt, metadata));

                // Writes a ServerHeartbeatResponse to the writer queue.
                _ = writer.TryWrite(BuildServerHeartbeatMessage(serverSentSequence, serverSentAt));

                StreamingHubPayloadPool.Shared.Return(payload);
            }
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
#if NON_UNITY
                timeProvider.GetUtcNow();
#else
                DateTimeOffset.UtcNow;
#endif

            StreamingHubMessageWriter.WriteClientHeartbeatMessage(buffer, clientSequence, now);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }

        public async ValueTask DisposeAsync()
        {
            Volatile.Write(ref disposed, true);
            shutdownTokenSource.Cancel();

            if (heartbeatLoopTask != null)
            {
                try
                {
                    await heartbeatLoopTask.ConfigureAwait(false);
                }
                catch
                {
                }
            }

            DisposeCore();

            void DisposeCore()
            {
                lock (gate)
                {
                    shutdownTokenSource.Dispose();
                    timeoutTokenSource.Dispose();
                }
            }
        }
    }
}
