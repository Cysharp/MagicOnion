#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace MagicOnion.Internal;

/// <summary>
/// A MethodId and its unreliable transport mode. This is the unit exchanged between peers during additional-transport negotiation.
/// </summary>
/// <param name="MethodId">The MethodId of the contract method.</param>
/// <param name="Mode">The unreliable transport mode of the contract method.</param>
internal readonly record struct StreamingHubTransportModeEntry(int MethodId, StreamingHubTransportMode Mode);

/// <summary>
/// A method known to both peers whose transport modes differ. Such a method is excluded from the agreed table and uses gRPC.
/// </summary>
/// <param name="MethodId">The MethodId of the contract method.</param>
/// <param name="LocalMode">The mode declared by the local contract.</param>
/// <param name="PeerMode">The mode declared by the peer contract.</param>
internal readonly record struct StreamingHubTransportModeConflict(int MethodId, StreamingHubTransportMode LocalMode, StreamingHubTransportMode PeerMode);

/// <summary>
/// An immutable table of unreliable methods for one direction of a StreamingHub connection, keyed by MethodId.
/// </summary>
/// <remarks>
/// <para>
/// Each peer sends the table of its own unreliable methods during negotiation. Both peers then compute the agreed table with
/// <see cref="Agree(StreamingHubTransportModeTable, StreamingHubTransportModeTable, out IReadOnlyList{StreamingHubTransportModeConflict})"/>:
/// the methods present on both sides with the same mode. Only agreed methods use the additional transport; every other unreliable
/// method uses gRPC for that connection. A method known to only one peer therefore behaves as it does today over gRPC,
/// which keeps the additional transport available across rolling upgrades.
/// </para>
/// <para>
/// Entries are sorted by MethodId in ascending signed order and are unique. Reliable entries are not allowed.
/// </para>
/// </remarks>
internal sealed class StreamingHubTransportModeTable
{
    /// <summary>Gets the empty table.</summary>
    public static StreamingHubTransportModeTable Empty { get; } = new StreamingHubTransportModeTable(Array.Empty<StreamingHubTransportModeEntry>());

    readonly StreamingHubTransportModeEntry[] entries;
#if NET8_0_OR_GREATER
    readonly FrozenDictionary<int, StreamingHubTransportMode> modesByMethodId;
#else
    readonly Dictionary<int, StreamingHubTransportMode> modesByMethodId;
#endif

    /// <summary>Gets the entries sorted by MethodId.</summary>
    public IReadOnlyList<StreamingHubTransportModeEntry> Entries => entries;

    /// <summary>Gets the number of entries.</summary>
    public int Count => entries.Length;

    StreamingHubTransportModeTable(StreamingHubTransportModeEntry[] sortedUniqueEntries)
    {
        entries = sortedUniqueEntries;
#if NET8_0_OR_GREATER
        modesByMethodId = sortedUniqueEntries.ToFrozenDictionary(static x => x.MethodId, static x => x.Mode);
#else
        modesByMethodId = sortedUniqueEntries.ToDictionary(static x => x.MethodId, static x => x.Mode);
#endif
    }

    /// <summary>
    /// Creates a table from entries. The entries are sorted by MethodId.
    /// </summary>
    /// <param name="entries">The unreliable methods. The order does not matter.</param>
    /// <exception cref="ArgumentException">An entry is reliable or has an undefined mode, or a MethodId appears more than once.</exception>
    public static StreamingHubTransportModeTable Create(IEnumerable<StreamingHubTransportModeEntry> entries)
    {
        if (entries is null) throw new ArgumentNullException(nameof(entries));

        var array = entries.ToArray();
        if (array.Length == 0) return Empty;

        Array.Sort(array, static (x, y) => x.MethodId.CompareTo(y.MethodId));
        for (var i = 0; i < array.Length; i++)
        {
            var entry = array[i];
            if (entry.Mode == StreamingHubTransportMode.Reliable || !Enum.IsDefined(typeof(StreamingHubTransportMode), entry.Mode))
            {
                throw new ArgumentException($"The mode of MethodId {entry.MethodId} must be an unreliable mode. (Mode: {entry.Mode})", nameof(entries));
            }
            if (i > 0 && array[i - 1].MethodId == entry.MethodId)
            {
                throw new ArgumentException($"MethodId {entry.MethodId} appears more than once.", nameof(entries));
            }
        }

        return new StreamingHubTransportModeTable(array);
    }

    /// <summary>
    /// Gets the unreliable transport mode of a method by MethodId.
    /// </summary>
    /// <param name="methodId">The MethodId.</param>
    /// <param name="mode">The mode when the table contains the method.</param>
    /// <returns><see langword="true"/> when the table contains the method; otherwise <see langword="false"/>.</returns>
    public bool TryGetMode(int methodId, out StreamingHubTransportMode mode)
        => modesByMethodId.TryGetValue(methodId, out mode);

    /// <summary>
    /// Computes the agreed table: the methods present in both tables with the same mode.
    /// </summary>
    /// <param name="local">The table of the local contract.</param>
    /// <param name="peer">The table received from the peer.</param>
    /// <param name="conflicts">Methods present in both tables with different modes. They are excluded from the result and should be reported.</param>
    /// <returns>The agreed table. Methods known to only one peer are not included and are not conflicts.</returns>
    public static StreamingHubTransportModeTable Agree(StreamingHubTransportModeTable local, StreamingHubTransportModeTable peer, out IReadOnlyList<StreamingHubTransportModeConflict> conflicts)
    {
        if (local is null) throw new ArgumentNullException(nameof(local));
        if (peer is null) throw new ArgumentNullException(nameof(peer));

        var agreed = new List<StreamingHubTransportModeEntry>(Math.Min(local.entries.Length, peer.entries.Length));
        List<StreamingHubTransportModeConflict>? conflictList = null;

        // Both arrays are sorted by MethodId, so a single merge pass finds the common methods.
        var i = 0;
        var j = 0;
        while (i < local.entries.Length && j < peer.entries.Length)
        {
            var l = local.entries[i];
            var p = peer.entries[j];
            if (l.MethodId < p.MethodId)
            {
                i++;
            }
            else if (l.MethodId > p.MethodId)
            {
                j++;
            }
            else
            {
                if (l.Mode == p.Mode)
                {
                    agreed.Add(l);
                }
                else
                {
                    (conflictList ??= new List<StreamingHubTransportModeConflict>()).Add(new StreamingHubTransportModeConflict(l.MethodId, l.Mode, p.Mode));
                }
                i++;
                j++;
            }
        }

        conflicts = conflictList ?? (IReadOnlyList<StreamingHubTransportModeConflict>)Array.Empty<StreamingHubTransportModeConflict>();
        return agreed.Count == 0 ? Empty : new StreamingHubTransportModeTable(agreed.ToArray());
    }

    /// <summary>
    /// Computes the agreed table without reporting conflicts.
    /// </summary>
    /// <param name="local">The table of the local contract.</param>
    /// <param name="peer">The table received from the peer.</param>
    public static StreamingHubTransportModeTable Agree(StreamingHubTransportModeTable local, StreamingHubTransportModeTable peer)
        => Agree(local, peer, out _);

    /// <summary>
    /// Determines whether every entry of <paramref name="other"/> exists in this table with the same mode.
    /// A client uses this to verify that the agreed table returned by the server is a subset of its own contract.
    /// </summary>
    /// <param name="other">The table to check.</param>
    public bool IsSupersetOf(StreamingHubTransportModeTable other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));

        foreach (var entry in other.entries)
        {
            if (!TryGetMode(entry.MethodId, out var mode) || mode != entry.Mode) return false;
        }
        return true;
    }

    /// <summary>
    /// Determines whether this table has exactly the same entries as <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The table to compare.</param>
    public bool ContentEquals(StreamingHubTransportModeTable other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        if (entries.Length != other.entries.Length) return false;

        for (var i = 0; i < entries.Length; i++)
        {
            if (entries[i] != other.entries[i]) return false;
        }
        return true;
    }
}
