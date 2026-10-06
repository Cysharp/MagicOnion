#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Internal;

/// <summary>
/// Describes a contract method together with its MethodId and effective transport mode.
/// </summary>
internal readonly struct StreamingHubTransportMethod
{
    /// <summary>Gets the MethodId of the contract method.</summary>
    public int MethodId { get; }

    /// <summary>Gets the effective transport mode of the contract method.</summary>
    public StreamingHubTransportMode Mode { get; }

    /// <summary>Gets the contract method declared on the shared interface.</summary>
    public MethodInfo Method { get; }

    public StreamingHubTransportMethod(int methodId, StreamingHubTransportMode mode, MethodInfo method)
    {
        MethodId = methodId;
        Mode = mode;
        Method = method;
    }
}

/// <summary>
/// Holds the resolved transport modes of a Hub or receiver contract interface.
/// </summary>
/// <remarks>
/// <para>
/// A contract never changes during the lifetime of the application, so <see cref="ForHub{THub}"/> and <see cref="ForReceiver{TReceiver}"/>
/// hold one instance per closed interface type in a generic static cache. Methods are sorted by MethodId.
/// </para>
/// <para>
/// <see cref="UnreliableMethods"/> is the table a peer sends during additional-transport negotiation. Both peers compute the agreed table
/// with <see cref="StreamingHubTransportModeTable.Agree(StreamingHubTransportModeTable, StreamingHubTransportModeTable, out IReadOnlyList{StreamingHubTransportModeConflict})"/>,
/// and only agreed methods use the additional transport. The table verifies delivery semantics; it does not verify argument types or
/// serializer compatibility.
/// </para>
/// </remarks>
internal sealed class StreamingHubTransportContract
{
    const string TrimmingMessage = "StreamingHubTransportContract enumerates contract interfaces by reflection and is incompatible with trimming. Use a generated client instead.";

#if NET8_0_OR_GREATER
    readonly FrozenDictionary<int, StreamingHubTransportMode> modesByMethodId;
#else
    readonly Dictionary<int, StreamingHubTransportMode> modesByMethodId;
#endif

    /// <summary>Gets the contract interface type.</summary>
    public Type InterfaceType { get; }

    /// <summary>Gets all contract methods sorted by MethodId.</summary>
    public IReadOnlyList<StreamingHubTransportMethod> Methods { get; }

    /// <summary>Gets the table of methods whose effective mode is unreliable. This is the table exchanged with the peer.</summary>
    public StreamingHubTransportModeTable UnreliableMethods { get; }

    /// <summary>Gets a value indicating whether the contract has at least one unreliable method.</summary>
    public bool HasUnreliableMethods => UnreliableMethods.Count > 0;

    StreamingHubTransportContract(Type interfaceType, List<StreamingHubTransportMethod> sortedMethods)
    {
        InterfaceType = interfaceType;
        Methods = sortedMethods.ToArray();
        UnreliableMethods = StreamingHubTransportModeTable.Create(sortedMethods
            .Where(static x => x.Mode != StreamingHubTransportMode.Reliable)
            .Select(static x => new StreamingHubTransportModeEntry(x.MethodId, x.Mode)));

        // MethodIds are already verified to be unique by Create.
#if NET8_0_OR_GREATER
        modesByMethodId = sortedMethods.ToFrozenDictionary(static x => x.MethodId, static x => x.Mode);
#else
        modesByMethodId = sortedMethods.ToDictionary(static x => x.MethodId, static x => x.Mode);
#endif
    }

    /// <summary>
    /// Gets the cached transport contract of a StreamingHub interface. Members of <see cref="IStreamingHub{TSelf, TReceiver}"/> are excluded.
    /// </summary>
    /// <typeparam name="THub">The closed Hub interface type.</typeparam>
    /// <exception cref="InvalidOperationException">The contract is invalid. The same exception is thrown on every access.</exception>
    [RequiresUnreferencedCode(TrimmingMessage)]
    public static StreamingHubTransportContract ForHub<THub>()
        => HubContractCache<THub>.Get();

    /// <summary>
    /// Gets the cached transport contract of a receiver interface.
    /// </summary>
    /// <typeparam name="TReceiver">The closed receiver interface type.</typeparam>
    /// <exception cref="InvalidOperationException">The contract is invalid. The same exception is thrown on every access.</exception>
    [RequiresUnreferencedCode(TrimmingMessage)]
    public static StreamingHubTransportContract ForReceiver<TReceiver>()
        => ReceiverContractCache<TReceiver>.Get();

    /// <summary>
    /// Gets the effective transport mode of a contract method by MethodId.
    /// </summary>
    /// <param name="methodId">The MethodId.</param>
    /// <param name="mode">The effective transport mode when the method exists.</param>
    /// <returns><see langword="true"/> when the contract contains the method; otherwise <see langword="false"/>.</returns>
    public bool TryGetMode(int methodId, out StreamingHubTransportMode mode)
        => modesByMethodId.TryGetValue(methodId, out mode);

    /// <summary>
    /// Gets the MethodId of a contract method, honoring <see cref="MethodIdAttribute"/> and falling back to the FNV-1a hash of the method name.
    /// </summary>
    /// <param name="method">The contract method.</param>
    public static int GetMethodId(MethodInfo method)
        => method.GetCustomAttribute<MethodIdAttribute>()?.MethodId ?? FNV1A32.GetHashCode(method.Name);

    [RequiresUnreferencedCode(TrimmingMessage)]
    static StreamingHubTransportContract Create(Type interfaceType, bool isHub)
    {
        if (!interfaceType.IsInterface) throw new ArgumentException($"The type '{interfaceType}' must be an interface.", nameof(interfaceType));

        var methods = new List<StreamingHubTransportMethod>();
        var methodsById = new Dictionary<int, MethodInfo>();
        foreach (var method in EnumerateContractMethods(interfaceType, isHub))
        {
            var methodId = GetMethodId(method);
            if (methodsById.TryGetValue(methodId, out var existing))
            {
                throw new InvalidOperationException(
                    $"'{interfaceType.Name}' does not allow duplicate MethodId (hash code) {methodId}. Please change the name or use MethodIdAttribute. ({existing.Name} and {method.Name})");
            }
            methodsById.Add(methodId, method);

            var mode = StreamingHubTransportModeResolver.Resolve(interfaceType, method);
            if (isHub)
            {
                StreamingHubTransportModeResolver.ValidateHubMethod(method, mode);
            }
            else
            {
                StreamingHubTransportModeResolver.ValidateReceiverMethod(method, mode);
            }

            methods.Add(new StreamingHubTransportMethod(methodId, mode, method));
        }

        methods.Sort(static (x, y) => x.MethodId.CompareTo(y.MethodId));
        return new StreamingHubTransportContract(interfaceType, methods);
    }

    [RequiresUnreferencedCode(TrimmingMessage)]
    static IEnumerable<MethodInfo> EnumerateContractMethods(Type interfaceType, bool isHub)
    {
        foreach (var declaringInterface in interfaceType.GetInterfaces().Concat(new[] { interfaceType }))
        {
            if (isHub && declaringInterface.IsGenericType && declaringInterface.GetGenericTypeDefinition() == typeof(IStreamingHub<,>)) continue;

            foreach (var method in declaringInterface.GetMethods())
            {
                if (method.IsSpecialName) continue;
                if (method.GetCustomAttribute<IgnoreAttribute>(false) is not null) continue;

                switch (method.Name)
                {
                    case "Equals":
                    case "GetHashCode":
                    case "GetType":
                    case "ToString":
                        continue;
                }

                if (isHub)
                {
                    switch (method.Name)
                    {
                        case "FireAndForget":
                        case "DisposeAsync":
                        case "WaitForDisconnect":
                            continue;
                    }
                }

                yield return method;
            }
        }
    }

    // Generic static caches: one contract per closed interface type, resolved by the runtime without a dictionary lookup.
    // A contract error is deterministic, so the original exception is kept and rethrown on every access
    // instead of surfacing as a TypeInitializationException that would hide the message.

    [RequiresUnreferencedCode(TrimmingMessage)]
    static class HubContractCache<THub>
    {
        static readonly StreamingHubTransportContract? contract;
        static readonly ExceptionDispatchInfo? error;

        static HubContractCache()
        {
            try
            {
                contract = Create(typeof(THub), isHub: true);
            }
            catch (Exception ex)
            {
                error = ExceptionDispatchInfo.Capture(ex);
            }
        }

        public static StreamingHubTransportContract Get()
        {
            error?.Throw();
            return contract!;
        }
    }

    [RequiresUnreferencedCode(TrimmingMessage)]
    static class ReceiverContractCache<TReceiver>
    {
        static readonly StreamingHubTransportContract? contract;
        static readonly ExceptionDispatchInfo? error;

        static ReceiverContractCache()
        {
            try
            {
                contract = Create(typeof(TReceiver), isHub: false);
            }
            catch (Exception ex)
            {
                error = ExceptionDispatchInfo.Capture(ex);
            }
        }

        public static StreamingHubTransportContract Get()
        {
            error?.Throw();
            return contract!;
        }
    }
}
