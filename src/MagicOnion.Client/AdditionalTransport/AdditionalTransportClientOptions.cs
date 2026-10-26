using MagicOnion.Internal;

namespace MagicOnion.Client;

/// <summary>
/// Configures the additional transports that a StreamingHub client offers to the server.
/// </summary>
/// <remarks>
/// A connection offers additional transports only when its hub or receiver contract has unreliable methods.
/// This API is experimental and may change.
/// </remarks>
public sealed class AdditionalTransportClientOptions
{
    /// <summary>
    /// Gets the default time limit of the negotiation after the server selects a candidate.
    /// </summary>
    public static TimeSpan DefaultNegotiationTimeout { get; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets the providers in order of preference.
    /// </summary>
    public IReadOnlyList<IAdditionalTransportClientProvider> Providers { get; }

    /// <summary>
    /// Gets whether the connection requires an additional transport.
    /// </summary>
    public AdditionalTransportEstablishment Establishment { get; }

    /// <summary>
    /// Gets the time limit of the negotiation after the server selects a candidate.
    /// When the limit expires, <c>ConnectAsync</c> fails regardless of <see cref="Establishment"/>.
    /// </summary>
    /// <remarks>
    /// Set a value longer than the negotiation timeout of the server, so that the server decides the fallback first.
    /// </remarks>
    public TimeSpan NegotiationTimeout { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdditionalTransportClientOptions"/> class.
    /// </summary>
    /// <param name="providers">The providers in order of preference. Up to 8 providers can be specified.</param>
    /// <param name="establishment">Whether the connection requires an additional transport.</param>
    /// <param name="negotiationTimeout">The time limit of the negotiation, or <see langword="null"/> to use <see cref="DefaultNegotiationTimeout"/>.</param>
    /// <exception cref="ArgumentException">A provider has an invalid identifier, a provider identifier and version are specified twice, or too many providers are specified.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="negotiationTimeout"/> is zero or negative.</exception>
    public AdditionalTransportClientOptions(IEnumerable<IAdditionalTransportClientProvider> providers, AdditionalTransportEstablishment establishment = AdditionalTransportEstablishment.Preferred, TimeSpan? negotiationTimeout = null)
    {
        if (providers is null) throw new ArgumentNullException(nameof(providers));
        if (establishment is not (AdditionalTransportEstablishment.Preferred or AdditionalTransportEstablishment.Required)) throw new ArgumentOutOfRangeException(nameof(establishment));
        if (negotiationTimeout is { } timeout && timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(negotiationTimeout), "The negotiation timeout must be positive.");

        var providerList = providers.ToArray();
        if (providerList.Length > AdditionalTransportNegotiation.MaxCandidates)
        {
            throw new ArgumentException($"Up to {AdditionalTransportNegotiation.MaxCandidates} additional transport providers can be specified.", nameof(providers));
        }

        var registered = new HashSet<(string, int)>();
        foreach (var provider in providerList)
        {
            if (provider is null) throw new ArgumentException("The providers must not contain null.", nameof(providers));
            if (!AdditionalTransportNegotiation.IsValidProviderId(provider.Id))
            {
                throw new ArgumentException($"The additional transport provider identifier '{provider.Id}' is invalid. Use 1 to 64 lowercase ASCII letters, digits, '.', '-', or '_'.", nameof(providers));
            }
            if (!registered.Add((provider.Id, provider.Version)))
            {
                throw new ArgumentException($"The additional transport provider '{provider.Id}' version {provider.Version} is specified more than once.", nameof(providers));
            }
        }

        Providers = providerList;
        Establishment = establishment;
        NegotiationTimeout = negotiationTimeout ?? DefaultNegotiationTimeout;
    }
}
