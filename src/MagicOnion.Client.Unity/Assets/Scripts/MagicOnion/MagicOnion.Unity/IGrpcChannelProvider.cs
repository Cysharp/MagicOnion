using System;
using System.Collections.Generic;
using Grpc.Core;

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

    public readonly struct CreateGrpcChannelContext
    {
        public IGrpcChannelProvider Provider { get; }
        public GrpcChannelTarget Target { get; }
        public ChannelOption[] ChannelOptions { get; }

        public CreateGrpcChannelContext(IGrpcChannelProvider provider, GrpcChannelTarget target, ChannelOption[] channelOptions)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Target = target;
            ChannelOptions = channelOptions ?? throw new ArgumentNullException(nameof(channelOptions));
        }
    }
}