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
        private readonly object _options;

        public GrpcChannelOptionsBag(object options)
        {
            _options = options;
        }

        public T Get<T>()
        {
            return TryGet<T>(out var value) ? value : default;
        }

        public bool TryGet<T>(out T value)
        {
            if (_options is T optionT)
            {
                value = optionT;
                return true;
            }

            value = default;
            return false;
        }

        public IEnumerable<KeyValuePair<string, object>> GetValues()
        {
#if USE_GRPC_NET_CLIENT
            if (TryGet<GrpcChannelOptions>(out var grpcChannelOptions))
            {
                yield return new KeyValuePair<string, object>(nameof(grpcChannelOptions.MaxReceiveMessageSize), grpcChannelOptions.MaxReceiveMessageSize ?? -1);
                yield return new KeyValuePair<string, object>(nameof(grpcChannelOptions.MaxRetryAttempts), grpcChannelOptions.MaxRetryAttempts ?? -1);
                yield return new KeyValuePair<string, object>(nameof(grpcChannelOptions.MaxRetryBufferPerCallSize), grpcChannelOptions.MaxRetryBufferPerCallSize ?? -1);
                yield return new KeyValuePair<string, object>(nameof(grpcChannelOptions.MaxRetryBufferSize), grpcChannelOptions.MaxRetryBufferSize ?? -1);
                yield return new KeyValuePair<string, object>(nameof(grpcChannelOptions.MaxSendMessageSize), grpcChannelOptions.MaxSendMessageSize ?? -1);
            }
#endif
#if !USE_GRPC_NET_CLIENT_ONLY
            if (TryGet<GrpcCCoreChannelOptions>(out var channelOptionsForCCore))
            {
                foreach (var channelOption in channelOptionsForCCore.ChannelOptions)
                {
                    yield return new KeyValuePair<string, object>(channelOption.Name, channelOption.Type == ChannelOption.OptionType.Integer ? (object)channelOption.IntValue : channelOption.StringValue);
                }
            }
#endif
        }
    }
}