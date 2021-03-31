using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Grpc.Core;
using UnityEngine;

namespace MagicOnion.Unity
{
    public static class GrpcChannelProviderExtensions
    {
        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        public static GrpcChannelx CreateChannel(this IGrpcChannelProvider provider, GrpcChannelTarget target, ChannelOption[] channelOptions = null)
            => provider.CreateChannel(new CreateGrpcChannelContext(provider, target, channelOptions ?? Array.Empty<ChannelOption>()));

        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        public static GrpcChannelx CreateChannel(this IGrpcChannelProvider provider, string host, int port, ChannelCredentials channelCredentials, ChannelOption[] channelOptions = null)
            => provider.CreateChannel(new GrpcChannelTarget(host, port, channelCredentials), channelOptions ?? Array.Empty<ChannelOption>());
    }

    /// <summary>
    /// Provide and manage gRPC channels for MagicOnion.
    /// </summary>
    public static class GrpcChannelProvider
    {
        private static IGrpcChannelProvider _defaultProvider;

        /// <summary>
        /// Gets a default channel provider. the provider will be initialized by <see cref="GrpcChannelProviderHost"/>.
        /// </summary>
        public static IGrpcChannelProvider Default
            => _defaultProvider ?? throw new InvalidOperationException("The default GrpcChannelProvider is not configured yet. Setup GrpcChannelProviderHost or initialize manually. ");

        public static void SetDefaultProvider(IGrpcChannelProvider provider)
        {
            _defaultProvider = provider;
        }
    }

    /// <summary>
    /// Provides logging for the channel provider.
    /// </summary>
    public sealed class LoggingGrpcChannelProvider : IGrpcChannelProvider
    {
        private readonly IGrpcChannelProvider _baseProvider;

        public LoggingGrpcChannelProvider(IGrpcChannelProvider baseProvider)
        {
            _baseProvider = baseProvider ?? throw new ArgumentNullException(nameof(baseProvider));
        }

        public GrpcChannelx CreateChannel(CreateGrpcChannelContext context)
        {
            var channel = _baseProvider.CreateChannel(context);
            Debug.Log($"Channel Created: {context.Target.Host}:{context.Target.Port} ({context.Target.ChannelCredentials}) [{channel.Id}]");
            return channel;
        }

        public IReadOnlyCollection<GrpcChannelx> GetAllChannels()
        {
            return _baseProvider.GetAllChannels();
        }

        public void UnregisterChannel(GrpcChannelx channel)
        {
            _baseProvider.UnregisterChannel(channel);
            Debug.Log($"Channel Unregistered: {channel.Target.Host}:{channel.Target.Port} [{channel.Id}]");
        }

        public void ShutdownAllChannels()
        {
            _baseProvider.ShutdownAllChannels();
        }
    }

    /// <summary>
    /// Provide and manage gRPC channels for MagicOnion.
    /// </summary>
    public sealed class DefaultGrpcChannelProvider : IGrpcChannelProvider
    {
        private readonly List<GrpcChannelx> _channels = new List<GrpcChannelx>();
        private readonly ChannelOption[] _defaultChannelOptions;
        private int _seq;

        public DefaultGrpcChannelProvider()
            : this(Array.Empty<ChannelOption>())
        {
        }

        public DefaultGrpcChannelProvider(ChannelOption[] channelOptions)
        {
            _defaultChannelOptions = channelOptions ?? throw new ArgumentNullException(nameof(channelOptions));
        }

        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        public GrpcChannelx CreateChannel(CreateGrpcChannelContext context)
        {
            var channelOptions = CreateGrpcChannelOptions(context.ChannelOptions);
            var id = Interlocked.Increment(ref _seq);
            var channel = new Channel(context.Target.Host, context.Target.Port, context.Target.ChannelCredentials, channelOptions);
            var channelHolder = new GrpcChannelx(
                id,
                context.Provider.UnregisterChannel /* Root provider may be wrapped outside this provider class. */,
                channel,
                new Uri((context.Target.ChannelCredentials == ChannelCredentials.Insecure ? "http" : "https") + $"://{context.Target.Host}:{context.Target.Port}"),
                channelOptions
            );

            lock (_channels)
            {
                _channels.Add(channelHolder);
            }

            return channelHolder;
        }

        private ChannelOption[] CreateGrpcChannelOptions(ChannelOption[] channelOptions)
        {
            if (channelOptions == null || channelOptions.Length == 0) return _defaultChannelOptions;

            return _defaultChannelOptions.Concat(channelOptions).ToArray();
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