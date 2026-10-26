using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeGen;

/// <summary>
/// Encodes the unreliable method tables embedded in generated StreamingHub clients.
/// </summary>
/// <remarks>
/// The format matches the entries of the runtime additional-transport negotiation: the MethodId as a big-endian 32-bit integer
/// followed by the mode as one byte. Entries are emitted in ascending MethodId order.
/// </remarks>
public static class TransportModeTableEncoding
{
    /// <summary>
    /// Gets the number of bytes of an entry.
    /// </summary>
    public const int EntryLength = 5;

    /// <summary>
    /// Encodes an entry.
    /// </summary>
    public static byte[] EncodeEntry(int methodId, TransportMode mode)
        => new[] { (byte)(methodId >> 24), (byte)(methodId >> 16), (byte)(methodId >> 8), (byte)methodId, (byte)mode };
}
