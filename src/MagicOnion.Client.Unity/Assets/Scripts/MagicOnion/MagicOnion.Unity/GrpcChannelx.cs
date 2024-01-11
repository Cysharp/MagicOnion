using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
#if MAGICONION_USE_GRPC_CCORE
using Channel = Grpc.Core.Channel;
#else
using Grpc.Net.Client;
#endif
using MagicOnion.Client;
using MagicOnion.Unity;
using UnityEngine;

namespace MagicOnion
{
    /// <summary>
    /// gRPC Channel wrapper that managed by the channel provider.
    /// </summary>
    public sealed partial class GrpcChannelx : ChannelBase, IMagicOnionAwareGrpcChannel, IDisposable
#if UNITY_EDITOR || MAGICONION_ENABLE_CHANNEL_DIAGNOSTICS
        , IGrpcChannelxDiagnosticsInfo
#endif
    {
        readonly Action<GrpcChannelx> onDispose;
        readonly Dictionary<IStreamingHubMarker, (Func<Task> DisposeAsync, ManagedStreamingHubInfo StreamingHubInfo)> streamingHubs = new Dictionary<IStreamingHubMarker, (Func<Task>, ManagedStreamingHubInfo)>();
        readonly ChannelBase channel;

        bool disposed;
        bool shutdownRequested;

        public Uri TargetUri { get; }
        public int Id { get; }

#if UNITY_EDITOR || MAGICONION_ENABLE_CHANNEL_DIAGNOSTICS
        readonly string stackTrace;
        readonly ChannelStats channelStats;
        readonly GrpcChannelOptionsBag channelOptions;

        string IGrpcChannelxDiagnosticsInfo.StackTrace => stackTrace;
        ChannelStats IGrpcChannelxDiagnosticsInfo.Stats => channelStats;
        GrpcChannelOptionsBag IGrpcChannelxDiagnosticsInfo.ChannelOptions => channelOptions;
        ChannelBase IGrpcChannelxDiagnosticsInfo.UnderlyingChannel => channel;
#endif

        public GrpcChannelx(int id, Action<GrpcChannelx> onDispose, ChannelBase channel, Uri targetUri, GrpcChannelOptionsBag channelOptions)
            : base(targetUri.ToString())
        {
            Id = id;
            TargetUri = targetUri;
            this.onDispose = onDispose;
            this.channel = channel;
            this.disposed = false;

#if UNITY_EDITOR || MAGICONION_ENABLE_CHANNEL_DIAGNOSTICS
            this.stackTrace = new System.Diagnostics.StackTrace().ToString();
            this.channelStats = new ChannelStats();
            this.channelOptions = channelOptions;
#endif
        }

        /// <summary>
        /// Create a channel to the specified target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [Obsolete("Use ForTarget instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static GrpcChannelx FromTarget(GrpcChannelTarget target)
            => GrpcChannelProvider.Default.CreateChannel(target);

        /// <summary>
        /// Create a channel to the specified target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [Obsolete("Use ForAddress instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static GrpcChannelx FromAddress(Uri target)
            => GrpcChannelProvider.Default.CreateChannel(new GrpcChannelTarget(target.Host, target.Port, target.Scheme == "http"));

        /// <summary>
        /// Create a channel to the specified target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static GrpcChannelx ForTarget(GrpcChannelTarget target)
            => GrpcChannelProvider.Default.CreateChannel(target);

        /// <summary>
        /// Create a channel to the specified target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static GrpcChannelx ForAddress(string target)
            => ForAddress(new Uri(target));

        /// <summary>
        /// Create a channel to the specified target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static GrpcChannelx ForAddress(Uri target)
            => GrpcChannelProvider.Default.CreateChannel(new GrpcChannelTarget(target.Host, target.Port, target.Scheme == "http"));

        /// <summary>
        /// Create a <see cref="CallInvoker"/>.
        /// </summary>
        /// <returns></returns>
        public override CallInvoker CreateCallInvoker()
        {
            ThrowIfDisposed();
#if UNITY_EDITOR || MAGICONION_ENABLE_CHANNEL_DIAGNOSTICS
            return new ChannelStats.WrappedCallInvoker(((IGrpcChannelxDiagnosticsInfo)this).Stats, channel.CreateCallInvoker());
#else
            return channel.CreateCallInvoker();
#endif
        }

        protected override async Task ShutdownAsyncCore()
        {
            await ShutdownInternalAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Connect to the target using gRPC channel. see <see cref="Grpc.Core.Channel.ConnectAsync"/>.
        /// </summary>
        /// <param name="deadline"></param>
        /// <returns></returns>
        [Obsolete]
#pragma warning disable CS1998
        public async Task ConnectAsync(DateTime? deadline = null)
#pragma warning restore CS1998
        {
            ThrowIfDisposed();
#if MAGICONION_USE_GRPC_CCORE
            if (channel is Channel grpcCChannel)
            {
                await grpcCChannel.ConnectAsync(deadline);
            }
#endif
        }

        /// <inheritdoc />
        IReadOnlyCollection<ManagedStreamingHubInfo> IMagicOnionAwareGrpcChannel.GetAllManagedStreamingHubs()
        {
            lock (streamingHubs)
            {
                return streamingHubs.Values.Select(x => x.StreamingHubInfo).ToArray();
            }
        }

        /// <inheritdoc />
        void IMagicOnionAwareGrpcChannel.ManageStreamingHubClient(Type streamingHubType, IStreamingHubMarker streamingHub, Func<Task> disposeAsync, Task waitForDisconnect)
        {
            lock (streamingHubs)
            {
                streamingHubs.Add(streamingHub, (disposeAsync, new ManagedStreamingHubInfo(streamingHubType, streamingHub)));

                // When the channel is disconnected, unregister it.
                Forget(WaitForDisconnectAndDisposeAsync(streamingHub, waitForDisconnect));
            }
        }

        private async Task WaitForDisconnectAndDisposeAsync(IStreamingHubMarker streamingHub, Task waitForDisconnect)
        {
            await waitForDisconnect;
            DisposeStreamingHubClient(streamingHub);
        }

        private void DisposeStreamingHubClient(IStreamingHubMarker streamingHub)
        {
            lock (streamingHubs)
            {
                if (streamingHubs.TryGetValue(streamingHub, out var disposeAsyncAndStreamingHubInfo))
                {
                    try
                    {
                        Forget(disposeAsyncAndStreamingHubInfo.DisposeAsync());
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    streamingHubs.Remove(streamingHub);
                }
            }

            async void Forget(Task t)
            {
                try
                {
                    await t;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void DisposeAllManagedStreamingHubs()
        {
            lock (streamingHubs)
            {
                foreach (var streamingHub in streamingHubs.Keys.ToArray() /* Snapshot */)
                {
                    DisposeStreamingHubClient(streamingHub);
                }
            }
        }

        public void Dispose()
        {
            if (disposed) return;

            disposed = true;
            try
            {
                DisposeAllManagedStreamingHubs();
                Forget(ShutdownInternalAsync());
            }
            finally
            {
                onDispose(this);
            }
        }

        public async Task DisposeAsync()
        {
            if (disposed) return;

            disposed = true;
            try
            {
                DisposeAllManagedStreamingHubs();
                await ShutdownInternalAsync();
            }
            finally
            {
                onDispose(this);
            }
        }

        private async Task ShutdownInternalAsync()
        {
            if (shutdownRequested) return;
            shutdownRequested = true;

            await channel.ShutdownAsync().ConfigureAwait(false);
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(GrpcChannelx));
        }

        private static async void Forget(Task t)
        {
            try
            {
                await t;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

#if UNITY_EDITOR || MAGICONION_ENABLE_CHANNEL_DIAGNOSTICS
        public class ChannelStats
        {
            int sentBytes = 0;
            int receivedBytes = 0;

            int indexSentBytes;
            int indexReceivedBytes;
            DateTime prevSentBytesAt;
            DateTime prevReceivedBytesAt;
            readonly int[] sentBytesHistory = new int[10];
            readonly int[] receivedBytesHistory = new int[10];

            public int SentBytes => sentBytes;
            public int ReceivedBytes => receivedBytes;

            public int SentBytesPerSecond
            {
                get
                {
                    AddValue(ref prevSentBytesAt, ref indexSentBytes, sentBytesHistory, DateTime.Now, 0);
                    return sentBytesHistory.Sum();
                }
            }

            public int ReceiveBytesPerSecond
            {
                get
                {
                    AddValue(ref prevReceivedBytesAt, ref indexReceivedBytes, receivedBytesHistory, DateTime.Now, 0);
                    return receivedBytesHistory.Sum();
                }
            }

            internal void AddSentBytes(int bytesLength)
            {
                Interlocked.Add(ref sentBytes, bytesLength);
                AddValue(ref prevSentBytesAt, ref indexSentBytes, sentBytesHistory, DateTime.Now, bytesLength);
            }

            internal void AddReceivedBytes(int bytesLength)
            {
                Interlocked.Add(ref receivedBytes, bytesLength);
                AddValue(ref prevReceivedBytesAt, ref indexReceivedBytes, receivedBytesHistory, DateTime.Now, bytesLength);
            }

            private void AddValue(ref DateTime prev, ref int index, int[] values, DateTime d, int value)
            {
                lock (values)
                {
                    var elapsed = d - prev;

                    if (elapsed.TotalMilliseconds > 1000)
                    {
                        index = 0;
                        Array.Clear(values, 0, values.Length);
                        prev = d;
                    }
                    else if (elapsed.TotalMilliseconds > 100)
                    {
                        var advance = (int)(elapsed.TotalMilliseconds / 100);
                        for (var i = 0; i < advance; i++)
                        {
                            values[(++index % values.Length)] = 0;
                        }
                        prev = d;
                    }

                    values[index % values.Length] += value;
                }
            }

            internal class WrappedCallInvoker : CallInvoker
            {
                readonly CallInvoker baseCallInvoker;
                readonly ChannelStats channelStats;


                public WrappedCallInvoker(ChannelStats channelStats, CallInvoker callInvoker)
                {
                    this.channelStats = channelStats;
                    this.baseCallInvoker = callInvoker;
                }

                public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
                {
                    //Debug.Log($"Unary(Blocking): {method.FullName}");
                    return baseCallInvoker.BlockingUnaryCall(WrapMethod(method), host, options, request);
                }

                public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
                {
                    //Debug.Log($"Unary: {method.FullName}");
                    return baseCallInvoker.AsyncUnaryCall(WrapMethod(method), host, options, request);
                }

                public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
                {
                    //Debug.Log($"ServerStreaming: {method.FullName}");
                    return baseCallInvoker.AsyncServerStreamingCall(WrapMethod(method), host, options, request);
                }

                public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
                {
                    //Debug.Log($"ClientStreaming: {method.FullName}");
                    return baseCallInvoker.AsyncClientStreamingCall(WrapMethod(method), host, options);
                }

                public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
                {
                    //Debug.Log($"DuplexStreaming: {method.FullName}");
                    return baseCallInvoker.AsyncDuplexStreamingCall(WrapMethod(method), host, options);
                }

                private Method<TRequest, TResponse> WrapMethod<TRequest, TResponse>(Method<TRequest, TResponse> method)
                {
                    var wrappedMethod = new Method<TRequest, TResponse>(
                        method.Type,
                        method.ServiceName,
                        method.Name,
                        new Marshaller<TRequest>((request, context) =>
                        {
                            var wrapper = new SerializationContextWrapper(context);
                            method.RequestMarshaller.ContextualSerializer(request, context);
                            channelStats.AddSentBytes(wrapper.Written);
                        }, (context) => method.RequestMarshaller.ContextualDeserializer(context)),
                        new Marshaller<TResponse>((request, context) => method.ResponseMarshaller.ContextualSerializer(request, context), x =>
                        {
                            channelStats.AddReceivedBytes(x.PayloadLength);
                            return method.ResponseMarshaller.ContextualDeserializer(x);
                        })
                    );

                    return wrappedMethod;
                }
            }

            private class SerializationContextWrapper : SerializationContext, IBufferWriter<byte>
            {
                readonly SerializationContext inner;
                IBufferWriter<byte>? bufferWriter;
                public int Written { get; private set; }

                public SerializationContextWrapper(SerializationContext inner)
                {
                    this.inner = inner;
                }

                public override IBufferWriter<byte> GetBufferWriter()
                    => bufferWriter ?? (bufferWriter = inner.GetBufferWriter());

                public override void Complete(byte[] payload)
                {
                    Written = payload.Length;
                    inner.Complete(payload);
                }

                public override void Complete()
                    => inner.Complete();

                public override void SetPayloadLength(int payloadLength)
                {
                    Written = payloadLength;
                    inner.SetPayloadLength(payloadLength);
                }

                public void Advance(int count)
                {
                    Written += count;
                    GetBufferWriter().Advance(count);
                }

                public Memory<byte> GetMemory(int sizeHint = 0) => GetBufferWriter().GetMemory(sizeHint);

                public Span<byte> GetSpan(int sizeHint = 0) => GetBufferWriter().GetSpan(sizeHint);
            }
        }
#endif
    }

#if UNITY_EDITOR || MAGICONION_ENABLE_CHANNEL_DIAGNOSTICS
    public interface IGrpcChannelxDiagnosticsInfo
    {
        string StackTrace { get; }

        GrpcChannelx.ChannelStats Stats { get; }

        GrpcChannelOptionsBag ChannelOptions { get; }

        ChannelBase UnderlyingChannel { get; }
    }
#endif
}
