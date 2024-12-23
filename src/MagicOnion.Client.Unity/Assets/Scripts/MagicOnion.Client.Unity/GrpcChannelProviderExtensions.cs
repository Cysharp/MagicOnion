namespace MagicOnion.Unity
{
    public static partial class GrpcChannelProviderExtensions
    {
        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        public static GrpcChannelx CreateChannel(this IGrpcChannelProvider provider, GrpcChannelTarget target)
            => provider.CreateChannel(new CreateGrpcChannelContext(provider, target));
    }
}
