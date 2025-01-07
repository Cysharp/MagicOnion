namespace MagicOnion.Client
{
    public static class StreamingHubClientExtensions
    {
        /// <summary>
        /// Wait for the disconnection and return the reason.
        /// </summary>
        public static Task<DisconnectionReason> WaitForDisconnectAsync<TStreamingHub>(this TStreamingHub hub) where TStreamingHub : IStreamingHubMarker =>
            CastOrThrow(hub).WaitForDisconnectAsync();

        static IStreamingHubClient CastOrThrow<THub>(THub hub)
        {
            if (hub is null)
            {
                throw new ArgumentNullException(nameof(hub));
            }
            if (hub is IStreamingHubClient client)
            {
                return client;
            }
            else
            {
                throw new InvalidOperationException($"The StreamingHub client '{hub.GetType().FullName}' does not implement IStreamingHubClient interface.");
            }
        }
    }
}
