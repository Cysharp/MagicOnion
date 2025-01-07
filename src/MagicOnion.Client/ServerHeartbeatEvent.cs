namespace MagicOnion.Client
{
    /// <summary>
    /// Represents a server heartbeat received event.
    /// </summary>
    public readonly struct ServerHeartbeatEvent
    {
        /// <summary>
        /// Gets the server time at when the heartbeat was sent.
        /// </summary>
        public DateTimeOffset ServerTime { get; }

        /// <summary>
        /// Gets the metadata data. The data is only available during event processing.
        /// </summary>
        public ReadOnlyMemory<byte> Metadata { get; }

        public ServerHeartbeatEvent(long serverTimeUnixMs, ReadOnlyMemory<byte> metadata)
        {
            ServerTime = DateTimeOffset.FromUnixTimeMilliseconds(serverTimeUnixMs);
            Metadata = metadata;
        }
    }
}
