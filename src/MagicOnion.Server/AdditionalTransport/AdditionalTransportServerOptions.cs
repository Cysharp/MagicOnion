namespace MagicOnion.Server;

/// <summary>
/// Options for negotiating additional transports of StreamingHub connections on the server.
/// </summary>
/// <remarks>This API is experimental and may change.</remarks>
public sealed class AdditionalTransportServerOptions
{
    TimeSpan negotiationTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the time limit of the negotiation, from receiving the offers until the result is finalized. Default is 5 seconds.
    /// </summary>
    /// <remarks>
    /// When the limit expires, the connection falls back to gRPC, or fails when the client requires an additional transport.
    /// Set a value shorter than the negotiation timeout of the clients, so that the server decides the fallback first.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value is zero or negative.</exception>
    public TimeSpan NegotiationTimeout
    {
        get => negotiationTimeout;
        set
        {
            if (value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(value), "The negotiation timeout must be positive.");
            negotiationTimeout = value;
        }
    }
}
