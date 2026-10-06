using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

/// <summary>
/// Resolves the effective <see cref="TransportMode"/> of StreamingHub and receiver methods from Roslyn symbols.
/// </summary>
/// <remarks>
/// This mirrors the runtime resolver (<c>StreamingHubTransportModeResolver</c>) used by the server and the dynamic client:
/// a transport attribute on the method declaration, then on the connected root interface, then the nearest inherited interface
/// default that applies to the method's declaring interface, and finally <see cref="TransportMode.Reliable"/>.
/// Conflicting attributes in the same scope and ambiguous inherited defaults at the same inheritance distance are reported as diagnostics.
/// </remarks>
internal static class TransportModeResolver
{
    const string TransportAttributeName = "TransportAttribute";
    const string UnreliableAttributeName = "UnreliableAttribute";
    const string UnreliableUnorderedAttributeName = "UnreliableUnorderedAttribute";
    const string ReliableAttributeName = "ReliableAttribute";

    /// <summary>
    /// Resolves the effective transport mode of a method accessed through the specified root contract interface.
    /// </summary>
    /// <param name="rootInterface">The connected Hub or receiver interface.</param>
    /// <param name="method">A method declared on <paramref name="rootInterface"/> or one of its inherited interfaces.</param>
    /// <param name="mode">The effective transport mode when resolution succeeds.</param>
    /// <param name="diagnostic">The diagnostic to report when the contract is invalid.</param>
    /// <returns><see langword="true"/> when the mode was resolved; otherwise <see langword="false"/>.</returns>
    public static bool TryResolve(ITypeSymbol rootInterface, IMethodSymbol method, out TransportMode mode, out Diagnostic? diagnostic)
    {
        mode = TransportMode.Reliable;

        // 1. The method declaration.
        if (!TryGetSingleMode(method, out var methodMode, out diagnostic)) return false;
        if (methodMode is { } resolvedByMethod)
        {
            mode = resolvedByMethod;
            return true;
        }

        // 2. The connected root interface.
        if (!TryGetSingleMode(rootInterface, out var rootMode, out diagnostic)) return false;
        if (rootMode is { } resolvedByRoot)
        {
            mode = resolvedByRoot;
            return true;
        }

        // 3. The nearest applicable inherited interface default.
        var declaringInterface = method.ContainingType;
        foreach (var level in EnumerateInheritedInterfacesByDistance(rootInterface))
        {
            TransportMode? levelMode = null;
            INamedTypeSymbol? levelSource = null;
            foreach (var inheritedInterface in level)
            {
                // The default of an interface applies only to the methods it declares or inherits.
                if (!IsSameOrDerivedFrom(inheritedInterface, declaringInterface)) continue;
                if (!TryGetSingleMode(inheritedInterface, out var candidate, out diagnostic)) return false;
                if (candidate is not { } candidateMode) continue;

                if (levelMode is { } existing && existing != candidateMode)
                {
                    diagnostic = Diagnostic.Create(
                        MagicOnionDiagnosticDescriptors.StreamingHubTransportModeAmbiguous,
                        method.Locations.FirstOrDefault(), null, null,
                        $"{declaringInterface.ToDisplayString()}.{method.Name}",
                        rootInterface.ToDisplayString(),
                        levelSource!.ToDisplayString(),
                        existing,
                        inheritedInterface.ToDisplayString(),
                        candidateMode);
                    return false;
                }

                levelMode = candidateMode;
                levelSource = inheritedInterface;
            }

            if (levelMode is { } resolvedByInheritance)
            {
                mode = resolvedByInheritance;
                return true;
            }
        }

        // 4. Default.
        mode = TransportMode.Reliable;
        return true;
    }

    static bool TryGetSingleMode(ISymbol symbol, out TransportMode? mode, out Diagnostic? diagnostic)
    {
        mode = null;
        diagnostic = null;

        List<(string Name, TransportMode Mode)>? found = null;
        foreach (var attribute in symbol.GetAttributes())
        {
            switch (attribute.AttributeClass?.Name)
            {
                case TransportAttributeName:
                    // The constructor argument is the enum value; Roslyn exposes it as the underlying integer.
                    var value = attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value is int intValue ? intValue : 0;
                    (found ??= new List<(string, TransportMode)>()).Add(("[Transport]", (TransportMode)value));
                    break;
                case UnreliableAttributeName:
                    (found ??= new List<(string, TransportMode)>()).Add(("[Unreliable]", TransportMode.UnreliableSequenced));
                    break;
                case UnreliableUnorderedAttributeName:
                    (found ??= new List<(string, TransportMode)>()).Add(("[UnreliableUnordered]", TransportMode.UnreliableUnordered));
                    break;
                case ReliableAttributeName:
                    (found ??= new List<(string, TransportMode)>()).Add(("[Reliable]", TransportMode.Reliable));
                    break;
            }
        }

        if (found is null) return true;

        if (found.Count > 1)
        {
            diagnostic = Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.StreamingHubTransportAttributeConflict,
                symbol.Locations.FirstOrDefault(), null, null,
                Describe(symbol),
                string.Join(", ", found.Select(x => x.Name)));
            return false;
        }

        mode = found[0].Mode;
        return true;
    }

    static string Describe(ISymbol symbol)
        => symbol is IMethodSymbol method
            ? $"{method.ContainingType.ToDisplayString()}.{method.Name}"
            : symbol.ToDisplayString();

    static bool IsSameOrDerivedFrom(INamedTypeSymbol candidate, INamedTypeSymbol declaringInterface)
    {
        if (SymbolEqualityComparer.Default.Equals(candidate, declaringInterface)) return true;
        foreach (var inherited in candidate.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(inherited, declaringInterface)) return true;
        }
        return false;
    }

    /// <summary>
    /// Enumerates the inherited interfaces of <paramref name="root"/> grouped by inheritance distance, nearest first.
    /// </summary>
    /// <remarks>
    /// Direct parents are derived from the flattened <see cref="ITypeSymbol.AllInterfaces"/> list rather than
    /// <see cref="ITypeSymbol.Interfaces"/>: for a type loaded from metadata the compiler stores every implemented interface,
    /// so the declared list alone would place indirectly inherited interfaces at distance 1. This mirrors the runtime resolver,
    /// which works on <c>Type.GetInterfaces()</c>, and keeps source and metadata contracts consistent.
    /// </remarks>
    static IEnumerable<INamedTypeSymbol[]> EnumerateInheritedInterfacesByDistance(ITypeSymbol root)
    {
        var allInterfaces = root.AllInterfaces;
        var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var frontier = new List<INamedTypeSymbol>();
        foreach (var parent in GetDirectParents(root, allInterfaces))
        {
            if (visited.Add(parent)) frontier.Add(parent);
        }

        while (frontier.Count > 0)
        {
            yield return frontier.ToArray();

            var next = new List<INamedTypeSymbol>();
            foreach (var type in frontier)
            {
                foreach (var parent in GetDirectParents(type, allInterfaces))
                {
                    if (visited.Add(parent)) next.Add(parent);
                }
            }
            frontier = next;
        }
    }

    /// <summary>
    /// Gets the interfaces that <paramref name="type"/> inherits directly, using only the flattened interface list of the root.
    /// An interface P is a direct parent when <paramref name="type"/> derives from P and no other interface in the list sits between them.
    /// </summary>
    static List<INamedTypeSymbol> GetDirectParents(ITypeSymbol type, ImmutableArray<INamedTypeSymbol> allInterfaces)
    {
        var direct = new List<INamedTypeSymbol>();
        foreach (var candidate in allInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(candidate, type) || !DerivesFrom(type, candidate)) continue;

            var isIndirect = false;
            foreach (var other in allInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(other, candidate) || SymbolEqualityComparer.Default.Equals(other, type)) continue;
                if (DerivesFrom(other, candidate) && DerivesFrom(type, other))
                {
                    isIndirect = true;
                    break;
                }
            }
            if (!isIndirect) direct.Add(candidate);
        }
        return direct;
    }

    static bool DerivesFrom(ITypeSymbol type, INamedTypeSymbol baseInterface)
        => type.AllInterfaces.Contains(baseInterface, SymbolEqualityComparer.Default);
}
