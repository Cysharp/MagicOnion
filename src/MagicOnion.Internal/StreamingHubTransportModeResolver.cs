using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MagicOnion.Internal;

/// <summary>
/// Resolves the effective <see cref="StreamingHubTransportMode"/> of StreamingHub and receiver methods from the shared contract interfaces.
/// </summary>
/// <remarks>
/// The effective mode is resolved in the following order:
/// <list type="number">
///     <item><description>A transport attribute on the method declaration.</description></item>
///     <item><description>A transport attribute directly on the connected root interface.</description></item>
///     <item><description>The nearest inherited interface default that applies to the method's declaring interface.</description></item>
///     <item><description><see cref="StreamingHubTransportMode.Reliable"/> when no attribute applies.</description></item>
/// </list>
/// Conflicting attributes in the same scope and ambiguous inherited defaults at the same inheritance distance are contract errors.
/// Attributes on implementation classes are never consulted.
/// </remarks>
internal static class StreamingHubTransportModeResolver
{
    /// <summary>
    /// Resolves the effective transport mode of a method when it is accessed through the specified root contract interface.
    /// </summary>
    /// <param name="rootInterface">The connected Hub or receiver interface.</param>
    /// <param name="method">A method declared on <paramref name="rootInterface"/> or one of its inherited interfaces.</param>
    /// <returns>The effective transport mode.</returns>
    /// <exception cref="InvalidOperationException">The contract specifies conflicting or ambiguous transport attributes.</exception>
    /// <remarks>
    /// Only the interface list of <paramref name="rootInterface"/> is reflected. Inheritance distances are derived from the flattened list
    /// with <see cref="Type.IsAssignableFrom"/>, so no further reflection on the inherited interfaces is required.
    /// </remarks>
    public static StreamingHubTransportMode Resolve(
#if NET5_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
        Type rootInterface,
        MethodInfo method)
    {
        if (rootInterface is null) throw new ArgumentNullException(nameof(rootInterface));
        if (method is null) throw new ArgumentNullException(nameof(method));
        if (!rootInterface.IsInterface) throw new ArgumentException($"The type '{rootInterface}' must be an interface.", nameof(rootInterface));

        var declaringType = method.DeclaringType ?? throw new ArgumentException($"The method '{method.Name}' has no declaring type.", nameof(method));
        if (!declaringType.IsAssignableFrom(rootInterface))
        {
            throw new ArgumentException($"The method '{declaringType.Name}.{method.Name}' is not a member of '{rootInterface.Name}' or its inherited interfaces.", nameof(method));
        }

        // 1. The method declaration.
        if (TryGetSingleMode(method, out var mode))
        {
            return mode;
        }

        // 2. The connected root interface.
        if (TryGetSingleMode(rootInterface, out mode))
        {
            return mode;
        }

        // 3. The nearest applicable inherited interface default.
        foreach (var level in EnumerateInheritedInterfacesByDistance(rootInterface, rootInterface.GetInterfaces()))
        {
            StreamingHubTransportMode? levelMode = null;
            Type? levelSource = null;
            foreach (var inheritedInterface in level)
            {
                // The default of an interface applies only to the methods it declares or inherits.
                if (!declaringType.IsAssignableFrom(inheritedInterface)) continue;
                if (!TryGetSingleMode(inheritedInterface, out var candidate)) continue;

                if (levelMode is { } existing && existing != candidate)
                {
                    throw new InvalidOperationException(
                        $"The transport mode of '{declaringType.Name}.{method.Name}' is ambiguous for '{rootInterface.Name}'. " +
                        $"'{levelSource!.Name}' specifies {existing} and '{inheritedInterface.Name}' specifies {candidate} at the same inheritance distance. " +
                        $"Specify the mode on the method or on '{rootInterface.Name}'.");
                }

                levelMode = candidate;
                levelSource = inheritedInterface;
            }

            if (levelMode is { } resolved)
            {
                return resolved;
            }
        }

        // 4. Default.
        return StreamingHubTransportMode.Reliable;
    }

    /// <summary>
    /// Validates that a Hub method is compatible with its effective transport mode.
    /// An unreliable Hub method must return <see langword="void"/>, <see cref="Task"/>, or <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="method">The Hub contract method.</param>
    /// <param name="mode">The effective transport mode.</param>
    /// <exception cref="InvalidOperationException">The method returns a result although it is unreliable.</exception>
    public static void ValidateHubMethod(MethodInfo method, StreamingHubTransportMode mode)
    {
        if (mode == StreamingHubTransportMode.Reliable) return;

        var returnType = method.ReturnType;
        if (returnType == typeof(void) || returnType == typeof(Task) || returnType == typeof(ValueTask)) return;

        throw new InvalidOperationException(
            $"The unreliable StreamingHub method '{method.DeclaringType?.Name}.{method.Name}' must return void, Task, or ValueTask. " +
            "A method that returns a result cannot be unreliable. Mark the method with [Reliable] to override an unreliable interface default.");
    }

    /// <summary>
    /// Validates that a receiver method is compatible with its effective transport mode.
    /// An unreliable receiver method must return <see langword="void"/>; a client result method cannot be unreliable.
    /// </summary>
    /// <param name="method">The receiver contract method.</param>
    /// <param name="mode">The effective transport mode.</param>
    /// <exception cref="InvalidOperationException">The method is a client result method although it is unreliable.</exception>
    public static void ValidateReceiverMethod(MethodInfo method, StreamingHubTransportMode mode)
    {
        if (mode == StreamingHubTransportMode.Reliable) return;
        if (method.ReturnType == typeof(void)) return;

        throw new InvalidOperationException(
            $"The unreliable receiver method '{method.DeclaringType?.Name}.{method.Name}' must return void. " +
            "A client result method cannot be unreliable. Mark the method with [Reliable] to override an unreliable interface default.");
    }

    static bool TryGetSingleMode(ICustomAttributeProvider member, out StreamingHubTransportMode mode)
    {
        var attributes = member.GetCustomAttributes(typeof(TransportAttribute), inherit: false);
        if (attributes.Length == 0)
        {
            mode = default;
            return false;
        }

        if (attributes.Length > 1)
        {
            var names = string.Join(", ", attributes.Select(x => "[" + x.GetType().Name.Replace("Attribute", string.Empty) + "]"));
            throw new InvalidOperationException(
                $"'{Describe(member)}' has multiple transport attributes ({names}). " +
                "Specify only one of [Transport], [Reliable], [Unreliable], or [UnreliableUnordered] on the same interface or method.");
        }

        mode = ((TransportAttribute)attributes[0]).Mode;
        return true;
    }

    static string Describe(ICustomAttributeProvider member)
        => member switch
        {
            Type type => type.Name,
            MethodInfo method => $"{method.DeclaringType?.Name}.{method.Name}",
            _ => member.ToString() ?? string.Empty,
        };

    /// <summary>
    /// Enumerates the inherited interfaces of <paramref name="root"/> grouped by inheritance distance, nearest first.
    /// </summary>
    /// <param name="root">The root interface.</param>
    /// <param name="allInterfaces">The flattened interface list of <paramref name="root"/>.</param>
    static IEnumerable<Type[]> EnumerateInheritedInterfacesByDistance(Type root, Type[] allInterfaces)
    {
        var visited = new HashSet<Type> { root };
        var frontier = new List<Type>();
        foreach (var parent in GetDirectParents(root, allInterfaces))
        {
            if (visited.Add(parent)) frontier.Add(parent);
        }

        while (frontier.Count > 0)
        {
            yield return frontier.ToArray();

            var next = new List<Type>();
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
    /// An interface P is a direct parent when <paramref name="type"/> implements P and no other interface in the list sits between them.
    /// </summary>
    static List<Type> GetDirectParents(Type type, Type[] allInterfaces)
    {
        var direct = new List<Type>();
        foreach (var candidate in allInterfaces)
        {
            if (candidate == type || !candidate.IsAssignableFrom(type)) continue;

            var isIndirect = false;
            foreach (var other in allInterfaces)
            {
                if (other == candidate || other == type) continue;
                if (candidate.IsAssignableFrom(other) && other.IsAssignableFrom(type))
                {
                    isIndirect = true;
                    break;
                }
            }
            if (!isIndirect) direct.Add(candidate);
        }
        return direct;
    }
}
