namespace MagicOnion.Client
{
    /// <summary>
    /// An interface that indicates that the StreamingHub client is implemented.
    /// </summary>
    public interface IStreamingHubClient
    {
        /// <summary>
        /// Wait for the disconnection and return the reason.
        /// </summary>
        /// <returns></returns>
        Task<DisconnectionReason> WaitForDisconnectAsync();
    }

    /// <summary>
    /// Provides the reason for the StreamingHub disconnection.
    /// </summary>
    public readonly struct DisconnectionReason
    {
        /// <summary>
        /// Gets the type of StreamingHub disconnection.
        /// </summary>
        public DisconnectionType Type { get; }

        /// <summary>
        /// Gets the exception that caused the disconnection.
        /// </summary>
        public Exception? Exception { get; }

        public DisconnectionReason(DisconnectionType type, Exception? exception)
        {
            Type = type;
            Exception = exception;
        }
    }

    /// <summary>
    /// Defines the types of StreamingHub disconnection.
    /// </summary>
    public enum DisconnectionType
    {
        /// <summary>
        /// Disconnected after completing successfully.
        /// </summary>
        CompletedNormally = 0,

        /// <summary>
        /// Disconnected due to exception while reading messages.
        /// </summary>
        Faulted = 1,

        /// <summary>
        /// Disconnected due to reaching the heartbeat timeout.
        /// </summary>
        TimedOut = 2,
    }

}
