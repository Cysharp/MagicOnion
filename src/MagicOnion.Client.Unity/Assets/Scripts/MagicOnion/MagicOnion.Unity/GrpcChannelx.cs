using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if MAGICONION_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using Channel = Grpc.Core.Channel;
#endif
using Grpc.Core;
#if USE_GRPC_NET_CLIENT
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
#if MAGICONION_UNITASK_SUPPORT
        , IUniTaskAsyncDisposable
#endif
    {
        private readonly Action<GrpcChannelx> _onDispose;
        private readonly Dictionary<IStreamingHubMarker, (Func<Task> DisposeAsync, ManagedStreamingHubInfo StreamingHubInfo)> _streamingHubs = new Dictionary<IStreamingHubMarker, (Func<Task>, ManagedStreamingHubInfo)>();
        private readonly ChannelBase _channel;
        private bool _disposed;

        public Uri TargetUri { get; }
        public int Id { get; }


#if UNITY_EDITOR || MAGICONION_ENABLE_CHANNEL_DIAGNOSTICS
        private readonly string _stackTrace;
        private readonly ChannelStats _channelStats;
        private readonly GrpcChannelOptionsBag _channelOptions;

        string IGrpcChannelxDiagnosticsInfo.StackTrace => _stackTrace;
        ChannelStats IGrpcChannelxDiagnosticsInfo.Stats => _channelStats;
        GrpcChannelOptionsBag IGrpcChannelxDiagnosticsInfo.ChannelOptions => _channelOptions;
        ChannelBase IGrpcChannelxDiagnosticsInfo.UnderlyingChannel => _channel;
#endif

        public GrpcChannelx(int id, Action<GrpcChannelx> onDispose, ChannelBase channel, Uri targetUri, GrpcChannelOptionsBag channelOptions)
            : base(targetUri.ToString())
        {
            Id = id;
            TargetUri = targetUri;
            _onDispose = onDispose;
            _channel = channel;
            _disposed = false;

#if UNITY_EDITOR || MAGICONION_ENABLE_CHANNEL_DIAGNOSTICS
            _stackTrace = new System.Diagnostics.StackTrace().ToString();
            _channelStats = new ChannelStats();
            _channelOptions = channelOptions;
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
            return new ChannelStats.WrappedCallInvoker(((IGrpcChannelxDiagnosticsInfo)this).Stats, _channel.CreateCallInvoker());
#else
            return _channel.CreateCallInvoker();
#endif
        }

        protected override async Task ShutdownAsyncCore()
        {
#if MAGICONION_UNITASK_SUPPORT
            await ShutdownInternalAsync();
#else
            await ShutdownInternalAsync().ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// Connect to the target using gRPC channel. see <see cref="Grpc.Core.Channel.ConnectAsync"/>.
        /// </summary>
        /// <param name="deadline"></param>
        /// <returns></returns>
        [Obsolete]
#if MAGICONION_UNITASK_SUPPORT
        public async UniTask ConnectAsync(DateTime? deadline = null)
#else
        public async Task ConnectAsync(DateTime? deadline = null)
#endif
        {
            ThrowIfDisposed();
#if !USE_GRPC_NET_CLIENT_ONLY
            if (_channel is Channel grpcCChannel)
            {
                await grpcCChannel.ConnectAsync(deadline);
            }
#endif
        }

        /// <inheritdoc />
        IReadOnlyCollection<ManagedStreamingHubInfo> IMagicOnionAwareGrpcChannel.GetAllManagedStreamingHubs()
        {
            lock (_streamingHubs)
            {
                return _streamingHubs.Values.Select(x => x.StreamingHubInfo).ToArray();
            }
        }

        /// <inheritdoc />
        void IMagicOnionAwareGrpcChannel.ManageStreamingHubClient(Type streamingHubType, IStreamingHubMarker streamingHub, Func<Task> disposeAsync, Task waitForDisconnect)
        {
            lock (_streamingHubs)
            {
                _streamingHubs.Add(streamingHub, (disposeAsync, new ManagedStreamingHubInfo(streamingHubType, streamingHub)));

                // When the channel is disconnected, unregister it.
                Forget(WaitForDisconnectAndDisposeAsync(streamingHub, waitForDisconnect));
            }
        }

#if MAGICONION_UNITASK_SUPPORT
        private async UniTask WaitForDisconnectAndDisposeAsync(IStreamingHubMarker streamingHub, Task waitForDisconnect)
#else
        private async Task WaitForDisconnectAndDisposeAsync(IStreamingHubMarker streamingHub, Task waitForDisconnect)
#endif
        {
            await waitForDisconnect;
            DisposeStreamingHubClient(streamingHub);
        }

        private void DisposeStreamingHubClient(IStreamingHubMarker streamingHub)
        {
            lock (_streamingHubs)
            {
                if (_streamingHubs.TryGetValue(streamingHub, out var disposeAsyncAndStreamingHubInfo))
                {
                    try
                    {
                        Forget(disposeAsyncAndStreamingHubInfo.DisposeAsync());
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    _streamingHubs.Remove(streamingHub);
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
            lock (_streamingHubs)
            {
                foreach (var streamingHub in _streamingHubs.Keys.ToArray() /* Snapshot */)
                {
                    DisposeStreamingHubClient(streamingHub);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            try
            {
                DisposeAllManagedStreamingHubs();
                Forget(ShutdownInternalAsync());
            }
            finally
            {
                _onDispose(this);
            }
        }

#if MAGICONION_UNITASK_SUPPORT
        public async UniTask DisposeAsync()
#else
        public async Task DisposeAsync()
#endif
        {
            if (_disposed) return;

            _disposed = true;
            try
            {
                DisposeAllManagedStreamingHubs();
                await ShutdownInternalAsync();
            }
            finally
            {
                _onDispose(this);
            }
        }

#if MAGICONION_UNITASK_SUPPORT
        private async UniTask ShutdownInternalAsync()
#else
        private async Task ShutdownInternalAsync()
#endif
        {
            await _channel.ShutdownAsync().ConfigureAwait(false);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(GrpcChannelx));
        }

#if MAGICONION_UNITASK_SUPPORT
        private static async void Forget(UniTask t)
            => t.Forget();
#endif

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
            private int _sentBytes = 0;
            private int _receivedBytes = 0;

            private int _indexSentBytes;
            private int _indexReceivedBytes;
            private DateTime _prevSentBytesAt;
            private DateTime _prevReceivedBytesAt;
            private readonly int[] _sentBytesHistory = new int[10];
            private readonly int[] _receivedBytesHistory = new int[10];

            public int SentBytes => _sentBytes;
            public int ReceivedBytes => _receivedBytes;

            public int SentBytesPerSecond
            {
                get
                {
                    AddValue(ref _prevSentBytesAt, ref _indexSentBytes, _sentBytesHistory, DateTime.Now, 0);
                    return _sentBytesHistory.Sum();
                }
            }

            public int ReceiveBytesPerSecond
            {
                get
                {
                    AddValue(ref _prevReceivedBytesAt, ref _indexReceivedBytes, _receivedBytesHistory, DateTime.Now, 0);
                    return _receivedBytesHistory.Sum();
                }
            }

            internal void AddSentBytes(int bytesLength)
            {
                Interlocked.Add(ref _sentBytes, bytesLength);
                AddValue(ref _prevSentBytesAt, ref _indexSentBytes, _sentBytesHistory, DateTime.Now, bytesLength);
            }

            internal void AddReceivedBytes(int bytesLength)
            {
                Interlocked.Add(ref _receivedBytes, bytesLength);
                AddValue(ref _prevReceivedBytesAt, ref _indexReceivedBytes, _receivedBytesHistory, DateTime.Now, bytesLength);
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
                private readonly CallInvoker _baseCallInvoker;
                private readonly ChannelStats _channelStats;


                public WrappedCallInvoker(ChannelStats channelStats, CallInvoker callInvoker)
                {
                    _channelStats = channelStats;
                    _baseCallInvoker = callInvoker;
                }

                public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
                {
                    //Debug.Log($"Unary(Blocking): {method.FullName}");
                    return _baseCallInvoker.BlockingUnaryCall(WrapMethod(method), host, options, request);
                }

                public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
                {
                    //Debug.Log($"Unary: {method.FullName}");
                    return _baseCallInvoker.AsyncUnaryCall(WrapMethod(method), host, options, request);
                }

                public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
                {
                    //Debug.Log($"ServerStreaming: {method.FullName}");
                    return _baseCallInvoker.AsyncServerStreamingCall(WrapMethod(method), host, options, request);
                }

                public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
                {
                    //Debug.Log($"ClientStreaming: {method.FullName}");
                    return _baseCallInvoker.AsyncClientStreamingCall(WrapMethod(method), host, options);
                }

                public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
                {
                    //Debug.Log($"DuplexStreaming: {method.FullName}");
                    return _baseCallInvoker.AsyncDuplexStreamingCall(WrapMethod(method), host, options);
                }

                private Method<TRequest, TResponse> WrapMethod<TRequest, TResponse>(Method<TRequest, TResponse> method)
                {
                    var wrappedMethod = new Method<TRequest, TResponse>(
                        method.Type,
                        method.ServiceName,
                        method.Name,
                        new Marshaller<TRequest>(x =>
                        {
                            var bytes = method.RequestMarshaller.Serializer(x);
                            _channelStats.AddSentBytes(bytes.Length);
                            return bytes;
                        }, x => method.RequestMarshaller.Deserializer(x)),
                        new Marshaller<TResponse>(x => method.ResponseMarshaller.Serializer(x), x =>
                        {
                            _channelStats.AddReceivedBytes(x.Length);
                            return method.ResponseMarshaller.Deserializer(x);
                        })
                    );

                    return wrappedMethod;
                }
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