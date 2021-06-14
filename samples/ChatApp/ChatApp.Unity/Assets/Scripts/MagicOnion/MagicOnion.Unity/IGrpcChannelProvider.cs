using System;
using System.Collections.Generic;
using Grpc.Core;
#if USE_GRPC_NET_CLIENT
using Grpc.Net.Client;
#endif

namespace MagicOnion.Unity
{
    /// <summary>
    /// Provide and manage gRPC channels for MagicOnion.
    /// </summary>
    public interface IGrpcChannelProvider
    {
        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        GrpcChannelx CreateChannel(CreateGrpcChannelContext ctx);

        /// <summary>
        /// Unregister the disposed channel. The method is called when the channel is disposed.
        /// </summary>
        /// <param name="channel"></param>
        void UnregisterChannel(GrpcChannelx channel);

        /// <summary>
        /// Returns all channels under the management of the provider.
        /// </summary>
        /// <returns></returns>
        IReadOnlyCollection<GrpcChannelx> GetAllChannels();

        /// <summary>
        ///  Shutdown all channels under the management of the provider.
        /// </summary>
        void ShutdownAllChannels();
    }

    public class CreateGrpcChannelContext
    {
        public IGrpcChannelProvider Provider { get; }
        public GrpcChannelTarget Target { get; }
        public GrpcChannelOptionsBag ChannelOptions { get; }

        public CreateGrpcChannelContext(IGrpcChannelProvider provider, GrpcChannelTarget target, object channelOptions = null)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Target = target;
            ChannelOptions = new GrpcChannelOptionsBag(channelOptions);
        }
    }

    public class GrpcChannelOptionsBag
    {
#if USE_GRPC_NET_CLIENT
        private readonly GrpcChannelOptions _grpcChannelOptions;
#endif
#if !USE_GRPC_NET_CLIENT_ONLY
        private readonly IReadOnlyList<ChannelOption> _grpcCChanelOptions;
#endif

        public GrpcChannelOptionsBag(object options)
        {
#if USE_GRPC_NET_CLIENT
            if (options is GrpcChannelOptions)
            {
                _grpcChannelOptions = (GrpcChannelOptions)options;
            }
#endif
#if !USE_GRPC_NET_CLIENT_ONLY
            if (options is IReadOnlyList<ChannelOption>)
            {
                _grpcCChanelOptions = (IReadOnlyList<ChannelOption>)options;
            }
#endif
        }

        public T Get<T>()
        {
            return TryGet<T>(out var value) ? value : default;
        }

        public bool TryGet<T>(out T value)
        {
#if USE_GRPC_NET_CLIENT
            if (typeof(T) == typeof(GrpcChannelOptions) && _grpcChannelOptions != null)
            {
                value = (T)(object)_grpcChannelOptions;
                return true;
            }
#endif
#if !USE_GRPC_NET_CLIENT_ONLY
            if (typeof(T) == typeof(IReadOnlyList<ChannelOption>) && _grpcCChanelOptions != null)
            {
                value = (T)(object)_grpcCChanelOptions;
                return true;
            }
#endif
            value = default;
            return false;
        }
    }
}