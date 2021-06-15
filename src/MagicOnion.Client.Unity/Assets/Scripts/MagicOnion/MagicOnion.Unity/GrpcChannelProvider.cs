using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Grpc.Core;
#if USE_GRPC_NET_CLIENT
using Grpc.Net.Client;
#endif
using UnityEngine;

namespace MagicOnion.Unity
{
    public static class GrpcChannelProviderExtensions
    {
        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        public static GrpcChannelx CreateChannel(this IGrpcChannelProvider provider, GrpcChannelTarget target)
            => provider.CreateChannel(new CreateGrpcChannelContext(provider, target));

#if USE_GRPC_NET_CLIENT
        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        public static GrpcChannelx CreateChannel(this IGrpcChannelProvider provider, GrpcChannelTarget target, GrpcChannelOptions channelOptions)
            => provider.CreateChannel(new CreateGrpcChannelContext(provider, target, channelOptions));
#endif
#if !USE_GRPC_NET_CLIENT_ONLY
        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        public static GrpcChannelx CreateChannel(this IGrpcChannelProvider provider, GrpcChannelTarget target, ChannelCredentials channelCredentials, IReadOnlyList<ChannelOption> channelOptions)
            => provider.CreateChannel(new CreateGrpcChannelContext(provider, target, new GrpcCCoreChannelOptions(channelOptions, channelCredentials)));
#endif
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
            Debug.Log($"Channel Created: {context.Target.Host}:{context.Target.Port} ({(context.Target.IsInsecure ? "Insecure" : "Secure")}) [{channel.Id}]");
            return channel;
        }

        public IReadOnlyCollection<GrpcChannelx> GetAllChannels()
        {
            return _baseProvider.GetAllChannels();
        }

        public void UnregisterChannel(GrpcChannelx channel)
        {
            _baseProvider.UnregisterChannel(channel);
            Debug.Log($"Channel Unregistered: {channel.TargetUri.Host}:{channel.TargetUri.Port} [{channel.Id}]");
        }

        public void ShutdownAllChannels()
        {
            _baseProvider.ShutdownAllChannels();
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

    /// <summary>
    /// Provide and manage gRPC channels for MagicOnion.
    /// </summary>
#if USE_GRPC_NET_CLIENT && USE_GRPC_NET_CLIENT_ONLY
    public class DefaultGrpcChannelProvider : GrpcNetClientGrpcChannelProvider
    {
        public DefaultGrpcChannelProvider() : base() {}
        public DefaultGrpcChannelProvider(GrpcChannelOptions channelOptions) : base(channelOptions) {}
    }
#else
    public class DefaultGrpcChannelProvider : GrpcCCoreGrpcChannelProvider
    {
        public DefaultGrpcChannelProvider() : base() { }
        public DefaultGrpcChannelProvider(IReadOnlyList<ChannelOption> channelOptions) : base(channelOptions) { }
        public DefaultGrpcChannelProvider(GrpcCCoreChannelOptions channelOptions) : base(channelOptions) { }
    }
#endif

#if USE_GRPC_NET_CLIENT
    /// <summary>
    /// Provide and manage gRPC channels using Grpc.Net.Client for MagicOnion.
    /// </summary>
    public class GrpcNetClientGrpcChannelProvider : GrpcChannelProviderBase
    {
        private readonly GrpcChannelOptions _defaultChannelOptions;
        public GrpcNetClientGrpcChannelProvider()
            : this(new GrpcChannelOptions())
        {
        }

        public GrpcNetClientGrpcChannelProvider(GrpcChannelOptions options)
        {
            _defaultChannelOptions = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        protected override GrpcChannelx CreateChannelCore(int id, CreateGrpcChannelContext context)
        {
            var address = new Uri((context.Target.IsInsecure ? "http" : "https") + $"://{context.Target.Host}:{context.Target.Port}");
            var channelOptions = context.ChannelOptions.Get<GrpcChannelOptions>() ?? _defaultChannelOptions;
            var channel = GrpcChannel.ForAddress(address, channelOptions);
            var channelHolder = new GrpcChannelx(
                id,
                context.Provider.UnregisterChannel /* Root provider may be wrapped outside this provider class. */,
                channel,
                address,
                new GrpcChannelOptionsBag(channelOptions)
            );

            return channelHolder;
        }
    }
#endif

#if !USE_GRPC_NET_CLIENT_ONLY
    /// <summary>
    /// Provide and manage gRPC channels using gRPC C-core for MagicOnion.
    /// </summary>
    public class GrpcCCoreGrpcChannelProvider : GrpcChannelProviderBase
    {
        private readonly GrpcCCoreChannelOptions _defaultChannelOptions;

        public GrpcCCoreGrpcChannelProvider()
            : this(new GrpcCCoreChannelOptions())
        {
        }

        public GrpcCCoreGrpcChannelProvider(IReadOnlyList<ChannelOption> channelOptions)
            : this(new GrpcCCoreChannelOptions(channelOptions))
        {
        }

        public GrpcCCoreGrpcChannelProvider(GrpcCCoreChannelOptions channelOptions)
        {
            _defaultChannelOptions = channelOptions ?? throw new ArgumentNullException(nameof(channelOptions));
        }

        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        protected override GrpcChannelx CreateChannelCore(int id, CreateGrpcChannelContext context)
        {
            var channelOptions = context.ChannelOptions.Get<GrpcCCoreChannelOptions>() ?? _defaultChannelOptions;
            var channel = new Channel(context.Target.Host, context.Target.Port, context.Target.IsInsecure ? ChannelCredentials.Insecure : channelOptions.ChannelCredentials, channelOptions.ChannelOptions);
            var channelHolder = new GrpcChannelx(
                id,
                context.Provider.UnregisterChannel /* Root provider may be wrapped outside this provider class. */,
                channel,
                new Uri((context.Target.IsInsecure ? "http" : "https") + $"://{context.Target.Host}:{context.Target.Port}"),
                new GrpcChannelOptionsBag(channelOptions)
            );

            return channelHolder;
        }
    }

    public sealed class GrpcCCoreChannelOptions
    {
        public IReadOnlyList<ChannelOption> ChannelOptions { get; set; }
        public ChannelCredentials ChannelCredentials { get; set; }

        public GrpcCCoreChannelOptions(IReadOnlyList<ChannelOption> channelOptions = null, ChannelCredentials channelCredentials = null)
        {
            ChannelOptions = channelOptions ?? Array.Empty<ChannelOption>();
            ChannelCredentials = channelCredentials ?? new SslCredentials();
        }
    }
#endif
}