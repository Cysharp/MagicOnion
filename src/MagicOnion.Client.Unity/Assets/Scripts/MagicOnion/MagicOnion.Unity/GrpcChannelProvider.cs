using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MagicOnion.Unity
{
    /// <summary>
    /// Provide and manage gRPC channels for MagicOnion.
    /// </summary>
    public static class GrpcChannelProvider
    {
        static IGrpcChannelProvider? defaultProvider;

        /// <summary>
        /// Gets a default channel provider. the provider will be initialized by <see cref="GrpcChannelProviderHost"/>.
        /// </summary>
        public static IGrpcChannelProvider Default
            => defaultProvider ?? throw new InvalidOperationException("The default GrpcChannelProvider is not configured yet. Setup GrpcChannelProviderHost or initialize manually. ");

        public static void SetDefaultProvider(IGrpcChannelProvider provider)
        {
            defaultProvider = provider;
        }
    }

    /// <summary>
    /// Provides logging for the channel provider.
    /// </summary>
    public sealed class LoggingGrpcChannelProvider : IGrpcChannelProvider
    {
        readonly IGrpcChannelProvider baseProvider;

        public LoggingGrpcChannelProvider(IGrpcChannelProvider baseProvider)
        {
            this.baseProvider = baseProvider ?? throw new ArgumentNullException(nameof(baseProvider));
        }

        public GrpcChannelx CreateChannel(CreateGrpcChannelContext context)
        {
            var channel = baseProvider.CreateChannel(context);
            Debug.Log($"Channel Created: {context.Target.Host}:{context.Target.Port} ({(context.Target.IsInsecure ? "Insecure" : "Secure")}) [{channel.Id}]");
            return channel;
        }

        public IReadOnlyCollection<GrpcChannelx> GetAllChannels()
        {
            return baseProvider.GetAllChannels();
        }

        public void UnregisterChannel(GrpcChannelx channel)
        {
            baseProvider.UnregisterChannel(channel);
            Debug.Log($"Channel Unregistered: {channel.TargetUri.Host}:{channel.TargetUri.Port} [{channel.Id}]");
        }

        public void ShutdownAllChannels()
        {
            baseProvider.ShutdownAllChannels();
        }
    }

    /// <summary>
    /// Provide and manage gRPC channels for MagicOnion.
    /// </summary>
    public abstract class GrpcChannelProviderBase : IGrpcChannelProvider
    {
        private readonly List<GrpcChannelx> _channels = new List<GrpcChannelx>();
        private int _seq;

        protected abstract GrpcChannelx CreateChannelCore(int id, CreateGrpcChannelContext context);

        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        public GrpcChannelx CreateChannel(CreateGrpcChannelContext context)
        {
            var channelHolder = CreateChannelCore(Interlocked.Increment(ref _seq), context);
            RegisterChannel(channelHolder);

            return channelHolder;
        }

        private void RegisterChannel(GrpcChannelx channel)
        {
            lock (_channels)
            {
                _channels.Add(channel);
            }
        }

        public void UnregisterChannel(GrpcChannelx channel)
        {
            lock (_channels)
            {
                _channels.Remove(channel);
            }
        }

        /// <summary>
        /// Returns all channels under the management of the provider.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<GrpcChannelx> GetAllChannels()
        {
            lock (_channels)
            {
                return _channels.ToArray();
            }
        }

        /// <summary>
        ///  Shutdown all channels under the management of the provider.
        /// </summary>
        public void ShutdownAllChannels()
        {
            lock (_channels)
            {
                foreach (var channel in _channels.ToArray() /* snapshot */)
                {
                    try
                    {
                        channel.Dispose();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
