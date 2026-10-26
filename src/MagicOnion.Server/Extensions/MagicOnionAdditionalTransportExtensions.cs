using System.Diagnostics.CodeAnalysis;
using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers additional transports for StreamingHub connections.
/// </summary>
/// <remarks>
/// <c>AddMagicOnion()</c> alone registers no additional transport. A server without a registered provider answers offers from clients
/// with a fallback to gRPC. This API is experimental and may change.
/// </remarks>
public static class MagicOnionAdditionalTransportExtensions
{
    /// <summary>
    /// Registers an additional-transport provider. The provider is created once by the service provider and shared by connections.
    /// </summary>
    /// <typeparam name="TProvider">The provider type.</typeparam>
    public static IMagicOnionServerBuilder AddAdditionalTransportProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TProvider>(this IMagicOnionServerBuilder builder)
        where TProvider : class, IAdditionalTransportServerProvider
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAdditionalTransportServerProvider, TProvider>());
        return builder;
    }

    /// <summary>
    /// Registers an additional-transport provider instance shared by connections.
    /// </summary>
    /// <param name="builder">The MagicOnion server builder.</param>
    /// <param name="provider">The provider instance.</param>
    public static IMagicOnionServerBuilder AddAdditionalTransportProvider(this IMagicOnionServerBuilder builder, IAdditionalTransportServerProvider provider)
    {
        if (provider is null) throw new ArgumentNullException(nameof(provider));
        builder.Services.AddSingleton(provider);
        return builder;
    }

    /// <summary>
    /// Configures the negotiation of additional transports.
    /// </summary>
    /// <param name="builder">The MagicOnion server builder.</param>
    /// <param name="configureOptions">The action to configure the options.</param>
    public static IMagicOnionServerBuilder ConfigureAdditionalTransport(this IMagicOnionServerBuilder builder, Action<AdditionalTransportServerOptions> configureOptions)
    {
        if (configureOptions is null) throw new ArgumentNullException(nameof(configureOptions));
        builder.Services.Configure(configureOptions);
        return builder;
    }
}
