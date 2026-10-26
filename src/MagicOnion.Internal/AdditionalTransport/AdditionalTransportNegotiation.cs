using System.Buffers.Binary;
using MagicOnion.Internal.Buffers;
using MessagePack;

namespace MagicOnion.Internal;

/// <summary>
/// Reasons reported during additional-transport negotiation. Values below 128 are part of the wire format.
/// </summary>
internal enum AdditionalTransportNegotiationReason : byte
{
    None = 0,
    /// <summary>No unreliable method is known to both peers with the same mode.</summary>
    NoAgreedMethods = 1,
    /// <summary>The server has no registered provider for any offer, or every matching provider rejected its offer.</summary>
    NoMatchingProvider = 2,
    /// <summary>Every accepted candidate failed to establish or activate.</summary>
    CandidatesExhausted = 3,
    /// <summary>The negotiation did not finish within the deadline.</summary>
    Timeout = 4,
    /// <summary>The peer uses an unsupported negotiation format version.</summary>
    UnsupportedVersion = 5,
    /// <summary>The client provider failed to establish the selected candidate.</summary>
    EstablishmentFailed = 6,
    /// <summary>The server provider rejected the activation of the selected candidate.</summary>
    ActivationRejected = 7,

    /// <summary>The server did not return a negotiation result, such as an older server. Never sent on the wire.</summary>
    NotSupportedByServer = 128,
}

/// <summary>
/// The negotiation status returned by the server in the response metadata.
/// </summary>
internal enum AdditionalTransportNegotiationStatus : byte
{
    /// <summary>A candidate was selected. The negotiation continues on the gRPC stream with control messages.</summary>
    Selected = 0,
    /// <summary>No candidate is available and the client allows gRPC. The connection is finalized as GrpcFallback.</summary>
    Fallback = 1,
    /// <summary>No candidate is available and the client requires an additional transport. The server ends the call.</summary>
    Failed = 2,
}

/// <summary>
/// Kinds of the control messages exchanged on the gRPC stream while an additional transport is being negotiated.
/// </summary>
/// <remarks>
/// <para>
/// Every control message uses the single StreamingHub sub-type 0x7d (AdditionalTransportControl), next to the heartbeats (0x7e, 0x7f),
/// and the kind is the element that follows the sub-type. Each kind has a single direction. The value 0 and values not listed here
/// are reserved, for example for provider-specific signaling, and a peer treats them as a protocol error.
/// </para>
/// </remarks>
internal enum AdditionalTransportControlMessageKind : byte
{
    /// <summary>Client to server: the selected candidate is established. Carries the provider activation data.</summary>
    Activate = 1,
    /// <summary>Client to server: the selected candidate could not be established. Carries the reason code.</summary>
    CandidateFailed = 2,

    /// <summary>Server to client: the server activated the candidate. The connection is finalized as ActiveAdditionalTransport.</summary>
    Activated = 3,
    /// <summary>Server to client: the server selected the next candidate. Carries the candidate index and the selection data.</summary>
    SelectNext = 4,
    /// <summary>Server to client: no candidate remains and the connection uses gRPC. The connection is finalized as GrpcFallback.</summary>
    GrpcSelected = 5,
    /// <summary>Server to client: no candidate remains and the client requires an additional transport. The server ends the call.</summary>
    NegotiationFailed = 6,
}

/// <summary>
/// An additional-transport candidate offered by the client.
/// </summary>
internal readonly struct AdditionalTransportOffer
{
    public string ProviderId { get; }
    public int ProviderVersion { get; }
    public ReadOnlyMemory<byte> Data { get; }

    public AdditionalTransportOffer(string providerId, int providerVersion, ReadOnlyMemory<byte> data)
    {
        ProviderId = providerId;
        ProviderVersion = providerVersion;
        Data = data;
    }
}

/// <summary>
/// The negotiation request sent by the client in the request metadata.
/// </summary>
internal sealed class AdditionalTransportNegotiationRequest
{
    public AdditionalTransportEstablishment Establishment { get; }
    public StreamingHubTransportModeTable ClientToServerMethods { get; }
    public StreamingHubTransportModeTable ServerToClientMethods { get; }
    public IReadOnlyList<AdditionalTransportOffer> Offers { get; }

    public AdditionalTransportNegotiationRequest(AdditionalTransportEstablishment establishment, StreamingHubTransportModeTable clientToServerMethods, StreamingHubTransportModeTable serverToClientMethods, IReadOnlyList<AdditionalTransportOffer> offers)
    {
        Establishment = establishment;
        ClientToServerMethods = clientToServerMethods;
        ServerToClientMethods = serverToClientMethods;
        Offers = offers;
    }
}

/// <summary>
/// The negotiation response returned by the server in the response metadata.
/// </summary>
internal sealed class AdditionalTransportNegotiationResponse
{
    public AdditionalTransportNegotiationStatus Status { get; }
    public AdditionalTransportNegotiationReason Reason { get; }
    /// <summary>Gets the attempt identifier of the selected candidate. Valid when <see cref="Status"/> is <see cref="AdditionalTransportNegotiationStatus.Selected"/>.</summary>
    public int AttemptId { get; }
    /// <summary>Gets the index of the selected candidate in the client's offers. Valid when selected.</summary>
    public int CandidateIndex { get; }
    /// <summary>Gets the provider selection data. Valid when selected.</summary>
    public ReadOnlyMemory<byte> Selection { get; }
    /// <summary>Gets the unreliable client-to-server methods agreed by both peers. Empty unless selected.</summary>
    public StreamingHubTransportModeTable AgreedClientToServerMethods { get; }
    /// <summary>Gets the unreliable server-to-client methods agreed by both peers. Empty unless selected.</summary>
    public StreamingHubTransportModeTable AgreedServerToClientMethods { get; }

    AdditionalTransportNegotiationResponse(AdditionalTransportNegotiationStatus status, AdditionalTransportNegotiationReason reason, int attemptId, int candidateIndex, ReadOnlyMemory<byte> selection, StreamingHubTransportModeTable agreedClientToServerMethods, StreamingHubTransportModeTable agreedServerToClientMethods)
    {
        Status = status;
        Reason = reason;
        AttemptId = attemptId;
        CandidateIndex = candidateIndex;
        Selection = selection;
        AgreedClientToServerMethods = agreedClientToServerMethods;
        AgreedServerToClientMethods = agreedServerToClientMethods;
    }

    public static AdditionalTransportNegotiationResponse Selected(int attemptId, int candidateIndex, ReadOnlyMemory<byte> selection, StreamingHubTransportModeTable agreedClientToServerMethods, StreamingHubTransportModeTable agreedServerToClientMethods)
        => new(AdditionalTransportNegotiationStatus.Selected, AdditionalTransportNegotiationReason.None, attemptId, candidateIndex, selection, agreedClientToServerMethods, agreedServerToClientMethods);

    public static AdditionalTransportNegotiationResponse Fallback(AdditionalTransportNegotiationReason reason)
        => new(AdditionalTransportNegotiationStatus.Fallback, reason, 0, 0, default, StreamingHubTransportModeTable.Empty, StreamingHubTransportModeTable.Empty);

    public static AdditionalTransportNegotiationResponse Failed(AdditionalTransportNegotiationReason reason)
        => new(AdditionalTransportNegotiationStatus.Failed, reason, 0, 0, default, StreamingHubTransportModeTable.Empty, StreamingHubTransportModeTable.Empty);
}

internal enum AdditionalTransportRequestDecodeResult
{
    Success,
    /// <summary>The request uses an unknown format version. Only the establishment policy could be read.</summary>
    UnsupportedVersion,
    Malformed,
}

/// <summary>
/// Encodes and decodes the additional-transport negotiation metadata.
/// </summary>
/// <remarks>
/// <para>The metadata is a sequence of MessagePack values without an enclosing array. The first two values of every request version are
/// the format version and the establishment policy. Both are below 128, so they occupy bytes 0 and 1 as positive fixints, and a server
/// can apply the client's policy to a request it cannot parse.</para>
/// <para>Request (version 1):</para>
/// <code>
/// version (u8), establishment (u8),
/// clientToServerMethods (bin), serverToClientMethods (bin),
/// offerCount (u8), offerCount x { providerId (str), providerVersion (i32), offer (bin) }
/// </code>
/// <para>Response (version 1):</para>
/// <code>
/// version (u8), status (u8), reason (u8),
/// when selected: attemptId (i32), candidateIndex (u8), selection (bin),
///                agreedClientToServerMethods (bin), agreedServerToClientMethods (bin)
/// </code>
/// <para>A method table is a bin of 5-byte entries: the MethodId as a big-endian 32-bit integer followed by the mode. Entries are sorted
/// by MethodId in ascending order without duplicates. Generated clients embed the same entries; see <see cref="EncodeEntries(StreamingHubTransportModeTable)"/>.</para>
/// </remarks>
internal static class AdditionalTransportNegotiation
{
    /// <summary>The key of the request and response metadata entry.</summary>
    public const string MetadataKey = "x-magiconion-additional-transport-bin";

    /// <summary>The negotiation format version that also identifies the delivery header and the Sequenced / Unordered semantics.</summary>
    public const byte FormatVersion = 1;

    public const int MaxMetadataLength = 4096;
    public const int MaxCandidates = 8;
    public const int MaxProviderDataLength = 2048;
    public const int MaxMethodEntries = 256;
    public const int MaxProviderIdLength = 64;
    public const int EntryLength = 5;

    /// <summary>
    /// Determines whether a provider identifier is valid: 1 to 64 characters of lowercase ASCII letters, digits, '.', '-', or '_'.
    /// </summary>
    public static bool IsValidProviderId(string? providerId)
        => providerId is { Length: > 0 and <= MaxProviderIdLength }
            && providerId.All(static c => c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '.' or '-' or '_');

    /// <summary>
    /// Encodes a negotiation request.
    /// </summary>
    /// <exception cref="InvalidOperationException">The request exceeds a negotiation limit.</exception>
    public static byte[] EncodeRequest(AdditionalTransportNegotiationRequest request)
    {
        if (request.Offers.Count > MaxCandidates) throw new InvalidOperationException($"The number of additional transport candidates exceeds the limit of {MaxCandidates}.");
        ThrowIfTooManyEntries(request.ClientToServerMethods);
        ThrowIfTooManyEntries(request.ServerToClientMethods);
        foreach (var offer in request.Offers)
        {
            if (!IsValidProviderId(offer.ProviderId)) throw new InvalidOperationException($"The additional transport provider identifier '{offer.ProviderId}' is invalid.");
            if (offer.Data.Length > MaxProviderDataLength) throw new InvalidOperationException($"The offer of the additional transport provider '{offer.ProviderId}' exceeds the limit of {MaxProviderDataLength} bytes.");
        }

        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        var writer = new MessagePackWriter(buffer);
        writer.Write(FormatVersion);
        writer.Write((byte)request.Establishment);
        WriteTable(ref writer, request.ClientToServerMethods);
        WriteTable(ref writer, request.ServerToClientMethods);
        writer.Write((byte)request.Offers.Count);
        foreach (var offer in request.Offers)
        {
            writer.Write(offer.ProviderId);
            writer.Write(offer.ProviderVersion);
            writer.Write(offer.Data.Span);
        }
        writer.Flush();
        return ToMetadata(buffer.WrittenSpan, "request");
    }

    /// <summary>
    /// Decodes a negotiation request. Offer data refers to <paramref name="data"/> without copying.
    /// </summary>
    /// <param name="data">The metadata value.</param>
    /// <param name="request">The decoded request when the result is <see cref="AdditionalTransportRequestDecodeResult.Success"/>.</param>
    /// <param name="establishment">The establishment policy when the result is <see cref="AdditionalTransportRequestDecodeResult.Success"/> or <see cref="AdditionalTransportRequestDecodeResult.UnsupportedVersion"/>.</param>
    public static AdditionalTransportRequestDecodeResult DecodeRequest(ReadOnlyMemory<byte> data, out AdditionalTransportNegotiationRequest? request, out AdditionalTransportEstablishment establishment)
    {
        request = null;
        establishment = default;
        if (data.Length > MaxMetadataLength) return AdditionalTransportRequestDecodeResult.Malformed;

        try
        {
            var reader = new MessagePackReader(data);
            var version = reader.ReadByte();
            var establishmentValue = reader.ReadByte();
            if (establishmentValue > (byte)AdditionalTransportEstablishment.Required) return AdditionalTransportRequestDecodeResult.Malformed;
            establishment = (AdditionalTransportEstablishment)establishmentValue;
            if (version != FormatVersion) return AdditionalTransportRequestDecodeResult.UnsupportedVersion;

            if (!TryReadTable(ref reader, out var clientToServer) || !TryReadTable(ref reader, out var serverToClient)) return AdditionalTransportRequestDecodeResult.Malformed;
            var offerCount = reader.ReadByte();
            if (offerCount > MaxCandidates) return AdditionalTransportRequestDecodeResult.Malformed;

            var offers = new AdditionalTransportOffer[offerCount];
            for (var i = 0; i < offers.Length; i++)
            {
                var providerId = reader.ReadString();
                var providerVersion = reader.ReadInt32();
                if (!IsValidProviderId(providerId) || !TryReadBin(ref reader, MaxProviderDataLength, out var offer)) return AdditionalTransportRequestDecodeResult.Malformed;
                offers[i] = new AdditionalTransportOffer(providerId!, providerVersion, offer);
            }
            if (!reader.End) return AdditionalTransportRequestDecodeResult.Malformed;

            request = new AdditionalTransportNegotiationRequest(establishment, clientToServer, serverToClient, offers);
            return AdditionalTransportRequestDecodeResult.Success;
        }
        catch (Exception ex) when (IsMalformedInput(ex))
        {
            return AdditionalTransportRequestDecodeResult.Malformed;
        }
    }

    /// <summary>
    /// Encodes a negotiation response.
    /// </summary>
    /// <exception cref="InvalidOperationException">The response exceeds a negotiation limit.</exception>
    public static byte[] EncodeResponse(AdditionalTransportNegotiationResponse response)
    {
        var selected = response.Status == AdditionalTransportNegotiationStatus.Selected;
        if (selected)
        {
            if (response.Selection.Length > MaxProviderDataLength) throw new InvalidOperationException($"The additional transport selection exceeds the limit of {MaxProviderDataLength} bytes.");
            if (response.CandidateIndex is < 0 or >= MaxCandidates) throw new InvalidOperationException("The candidate index is out of range.");
            ThrowIfTooManyEntries(response.AgreedClientToServerMethods);
            ThrowIfTooManyEntries(response.AgreedServerToClientMethods);
        }

        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        var writer = new MessagePackWriter(buffer);
        writer.Write(FormatVersion);
        writer.Write((byte)response.Status);
        writer.Write((byte)response.Reason);
        if (selected)
        {
            writer.Write(response.AttemptId);
            writer.Write((byte)response.CandidateIndex);
            writer.Write(response.Selection.Span);
            WriteTable(ref writer, response.AgreedClientToServerMethods);
            WriteTable(ref writer, response.AgreedServerToClientMethods);
        }
        writer.Flush();
        return ToMetadata(buffer.WrittenSpan, "response");
    }

    /// <summary>
    /// Decodes a negotiation response. The selection data refers to <paramref name="data"/> without copying.
    /// </summary>
    /// <returns><see langword="false"/> when the response is malformed or uses an unsupported version.</returns>
    public static bool TryDecodeResponse(ReadOnlyMemory<byte> data, out AdditionalTransportNegotiationResponse? response)
    {
        response = null;
        if (data.Length > MaxMetadataLength) return false;

        try
        {
            var reader = new MessagePackReader(data);
            if (reader.ReadByte() != FormatVersion) return false;
            var statusValue = reader.ReadByte();
            var reasonValue = reader.ReadByte();
            if (statusValue > (byte)AdditionalTransportNegotiationStatus.Failed || reasonValue >= 128) return false;

            var status = (AdditionalTransportNegotiationStatus)statusValue;
            var reason = (AdditionalTransportNegotiationReason)reasonValue;
            if (status != AdditionalTransportNegotiationStatus.Selected)
            {
                if (!reader.End) return false;
                response = status == AdditionalTransportNegotiationStatus.Fallback
                    ? AdditionalTransportNegotiationResponse.Fallback(reason)
                    : AdditionalTransportNegotiationResponse.Failed(reason);
                return true;
            }

            var attemptId = reader.ReadInt32();
            var candidateIndex = reader.ReadByte();
            if (candidateIndex >= MaxCandidates) return false;
            if (!TryReadBin(ref reader, MaxProviderDataLength, out var selection)) return false;
            if (!TryReadTable(ref reader, out var agreedClientToServer) || !TryReadTable(ref reader, out var agreedServerToClient)) return false;
            if (!reader.End) return false;

            response = AdditionalTransportNegotiationResponse.Selected(attemptId, candidateIndex, selection, agreedClientToServer, agreedServerToClient);
            return true;
        }
        catch (Exception ex) when (IsMalformedInput(ex))
        {
            return false;
        }
    }

    /// <summary>
    /// Encodes the entries of a table as raw 5-byte entries, as embedded in generated clients.
    /// </summary>
    public static byte[] EncodeEntries(StreamingHubTransportModeTable table)
    {
        var encoded = new byte[table.Count * EntryLength];
        EncodeEntries(table, encoded);
        return encoded;
    }

    /// <summary>
    /// Decodes raw 5-byte entries, as embedded in generated clients.
    /// </summary>
    /// <exception cref="InvalidOperationException">The entries are not in the canonical form.</exception>
    public static StreamingHubTransportModeTable DecodeEntries(ReadOnlySpan<byte> encoded)
        => TryDecodeEntries(encoded, out var table)
            ? table
            : throw new InvalidOperationException("The encoded table of unreliable methods is invalid. Entries must be sorted by MethodId without duplicates and specify an unreliable mode. Regenerate the client.");

    static void EncodeEntries(StreamingHubTransportModeTable table, Span<byte> destination)
    {
        foreach (var entry in table.Entries)
        {
            BinaryPrimitives.WriteInt32BigEndian(destination, entry.MethodId);
            destination[4] = (byte)entry.Mode;
            destination = destination.Slice(EntryLength);
        }
    }

    /// <summary>
    /// Decodes raw 5-byte entries. Fails unless the entries are in the canonical form: sorted by MethodId without duplicates, each with an unreliable mode.
    /// </summary>
    static bool TryDecodeEntries(ReadOnlySpan<byte> encoded, out StreamingHubTransportModeTable table)
    {
        table = StreamingHubTransportModeTable.Empty;
        if (encoded.Length % EntryLength != 0) return false;
        if (encoded.IsEmpty) return true;

        var entries = new StreamingHubTransportModeEntry[encoded.Length / EntryLength];
        for (var i = 0; i < entries.Length; i++)
        {
            var entry = encoded.Slice(i * EntryLength, EntryLength);
            var methodId = BinaryPrimitives.ReadInt32BigEndian(entry);
            var mode = (StreamingHubTransportMode)entry[4];
            if (mode is not (StreamingHubTransportMode.UnreliableSequenced or StreamingHubTransportMode.UnreliableUnordered)) return false;
            if (i > 0 && entries[i - 1].MethodId >= methodId) return false;
            entries[i] = new StreamingHubTransportModeEntry(methodId, mode);
        }

        table = StreamingHubTransportModeTable.Create(entries);
        return true;
    }

    static void WriteTable(ref MessagePackWriter writer, StreamingHubTransportModeTable table)
    {
        var length = table.Count * EntryLength;
        writer.WriteBinHeader(length);
        if (length == 0) return;

        EncodeEntries(table, writer.GetSpan(length));
        writer.Advance(length);
    }

    static bool TryReadTable(ref MessagePackReader reader, out StreamingHubTransportModeTable table)
    {
        table = StreamingHubTransportModeTable.Empty;
        return TryReadBin(ref reader, MaxMethodEntries * EntryLength, out var encoded) && TryDecodeEntries(encoded.Span, out table);
    }

    static bool TryReadBin(ref MessagePackReader reader, int maxLength, out ReadOnlyMemory<byte> value)
    {
        // The reader is created over a single memory, so a bin value is a single segment that refers to it without copying.
        if (reader.ReadBytes() is { } bytes && bytes.Length <= maxLength)
        {
            value = bytes.First;
            return true;
        }

        value = default;
        return false;
    }

    static byte[] ToMetadata(ReadOnlySpan<byte> written, string kind)
    {
        if (written.Length > MaxMetadataLength) throw new InvalidOperationException($"The additional transport negotiation {kind} exceeds the limit of {MaxMetadataLength} bytes.");
        return written.ToArray();
    }

    static void ThrowIfTooManyEntries(StreamingHubTransportModeTable table)
    {
        if (table.Count > MaxMethodEntries) throw new InvalidOperationException($"The number of unreliable methods exceeds the limit of {MaxMethodEntries}.");
    }

    // MessagePackReader reports truncated input, unexpected value types, and out-of-range integers with these exceptions.
    static bool IsMalformedInput(Exception ex)
        => ex is MessagePackSerializationException or EndOfStreamException or OverflowException;
}
