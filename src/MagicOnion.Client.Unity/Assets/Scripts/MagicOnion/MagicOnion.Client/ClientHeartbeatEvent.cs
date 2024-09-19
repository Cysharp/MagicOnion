using System;

namespace MagicOnion.Client
{
    /// <summary>
    /// Represents a client heartbeat received event.
    /// </summary>
    public readonly struct ClientHeartbeatEvent
    {
        /// <summary>
        /// Gets the round trip time (RTT) between client and server.
        /// </summary>
        public TimeSpan RoundTripTime { get; }

        public ClientHeartbeatEvent(long roundTripTimeMs)
        {
            RoundTripTime = TimeSpan.FromMilliseconds(roundTripTimeMs);
        }
    }
}
