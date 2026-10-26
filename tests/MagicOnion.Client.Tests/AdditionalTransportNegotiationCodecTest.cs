using System.Buffers;
using System.Buffers.Binary;
using MagicOnion.Internal;
using MessagePack;

namespace MagicOnion.Client.Tests;

public class AdditionalTransportNegotiationCodecTest
{
    static StreamingHubTransportModeTable Table(params (int MethodId, StreamingHubTransportMode Mode)[] entries)
        => StreamingHubTransportModeTable.Create(entries.Select(x => new StreamingHubTransportModeEntry(x.MethodId, x.Mode)));

    static AdditionalTransportNegotiationRequest CreateRequest(AdditionalTransportEstablishment establishment = AdditionalTransportEstablishment.Preferred, params AdditionalTransportOffer[] offers)
        => new(establishment,
            Table((-5, StreamingHubTransportMode.UnreliableSequenced), (10, StreamingHubTransportMode.UnreliableUnordered)),
            Table((3, StreamingHubTransportMode.UnreliableSequenced)),
            offers.Length == 0 ? [new AdditionalTransportOffer("magiconion.transport.udp", 1, new byte[] { 1, 2, 3 })] : offers);

    [Fact]
    public void Request_RoundTrip()
    {
        var request = CreateRequest(AdditionalTransportEstablishment.Required,
            new AdditionalTransportOffer("magiconion.transport.udp", 1, new byte[] { 1, 2, 3 }),
            new AdditionalTransportOffer("example-transport_2", -7, ReadOnlyMemory<byte>.Empty));

        var encoded = AdditionalTransportNegotiation.EncodeRequest(request);
        var result = AdditionalTransportNegotiation.DecodeRequest(encoded, out var decoded, out var establishment);

        Assert.Equal(AdditionalTransportRequestDecodeResult.Success, result);
        Assert.Equal(AdditionalTransportEstablishment.Required, establishment);
        Assert.NotNull(decoded);
        Assert.Equal(AdditionalTransportEstablishment.Required, decoded.Establishment);
        Assert.True(decoded.ClientToServerMethods.ContentEquals(request.ClientToServerMethods));
        Assert.True(decoded.ServerToClientMethods.ContentEquals(request.ServerToClientMethods));
        Assert.Equal(2, decoded.Offers.Count);
        Assert.Equal("magiconion.transport.udp", decoded.Offers[0].ProviderId);
        Assert.Equal(1, decoded.Offers[0].ProviderVersion);
        Assert.Equal(new byte[] { 1, 2, 3 }, decoded.Offers[0].Data.ToArray());
        Assert.Equal("example-transport_2", decoded.Offers[1].ProviderId);
        Assert.Equal(-7, decoded.Offers[1].ProviderVersion);
        Assert.True(decoded.Offers[1].Data.IsEmpty);
    }

    [Fact]
    public void Request_Layout_StartsWithVersionAndPolicyAsSingleBytes()
    {
        var request = CreateRequest(AdditionalTransportEstablishment.Required);
        var encoded = AdditionalTransportNegotiation.EncodeRequest(request);

        // A server that cannot parse a newer version still reads the policy from the first two bytes.
        Assert.Equal(AdditionalTransportNegotiation.FormatVersion, encoded[0]);
        Assert.Equal((byte)AdditionalTransportEstablishment.Required, encoded[1]);

        // A table is a bin value of the same 5-byte entries that generated clients embed.
        var reader = new MessagePackReader(encoded.AsMemory(2));
        Assert.Equal(AdditionalTransportNegotiation.EncodeEntries(request.ClientToServerMethods), reader.ReadBytes()!.Value.ToArray());
        Assert.Equal(AdditionalTransportNegotiation.EncodeEntries(request.ServerToClientMethods), reader.ReadBytes()!.Value.ToArray());
    }

    [Fact]
    public void Request_UnsupportedVersion_ReportsEstablishment()
    {
        var encoded = AdditionalTransportNegotiation.EncodeRequest(CreateRequest(AdditionalTransportEstablishment.Required));
        encoded[0] = 99;

        var result = AdditionalTransportNegotiation.DecodeRequest(encoded, out var decoded, out var establishment);

        Assert.Equal(AdditionalTransportRequestDecodeResult.UnsupportedVersion, result);
        Assert.Null(decoded);
        Assert.Equal(AdditionalTransportEstablishment.Required, establishment);
    }

    [Fact]
    public void Request_Malformed()
    {
        var valid = AdditionalTransportNegotiation.EncodeRequest(CreateRequest());

        // Every truncation is rejected.
        for (var length = 0; length < valid.Length; length++)
        {
            Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(valid.AsMemory(0, length), out _, out _));
        }

        // Trailing bytes.
        Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(valid.Concat(new byte[] { 0 }).ToArray(), out _, out _));

        // Unknown establishment policy.
        var invalidPolicy = valid.ToArray();
        invalidPolicy[1] = 2;
        Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(invalidPolicy, out _, out _));

        // The first table follows the version, the policy, and its 2-byte bin8 header.
        Assert.Equal(MessagePackCode.Bin8, valid[2]);
        const int firstEntry = 4;
        const int secondEntry = firstEntry + AdditionalTransportNegotiation.EntryLength;

        // Reliable mode in a table.
        var reliableMode = valid.ToArray();
        reliableMode[firstEntry + 4] = (byte)StreamingHubTransportMode.Reliable;
        Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(reliableMode, out _, out _));

        // Entries out of order (swap the MethodIds -5 and 10).
        var unsorted = valid.ToArray();
        BinaryPrimitives.WriteInt32BigEndian(unsorted.AsSpan(firstEntry), 10);
        BinaryPrimitives.WriteInt32BigEndian(unsorted.AsSpan(secondEntry), -5);
        Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(unsorted, out _, out _));

        // Duplicate MethodIds.
        var duplicate = valid.ToArray();
        BinaryPrimitives.WriteInt32BigEndian(duplicate.AsSpan(secondEntry), -5);
        Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(duplicate, out _, out _));

        // A table that is nil, and a table that is an integer instead of a bin value.
        var nilTable = valid.ToArray();
        nilTable[2] = MessagePackCode.Nil;
        Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(nilTable, out _, out _));
        var integerTable = valid.ToArray();
        integerTable[2] = MessagePackCode.UInt8;
        Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(integerTable, out _, out _));

        // An invalid character in a provider identifier.
        var invalidId = valid.ToArray();
        var idOffset = Array.IndexOf(invalidId, (byte)'m');
        invalidId[idOffset] = (byte)'M';
        Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(invalidId, out _, out _));

        // Longer than the metadata limit.
        Assert.Equal(AdditionalTransportRequestDecodeResult.Malformed, AdditionalTransportNegotiation.DecodeRequest(new byte[AdditionalTransportNegotiation.MaxMetadataLength + 1], out _, out _));
    }

    [Fact]
    public void Request_Encode_EnforcesLimits()
    {
        var tooManyOffers = Enumerable.Range(0, AdditionalTransportNegotiation.MaxCandidates + 1)
            .Select(x => new AdditionalTransportOffer($"provider{x}", 1, ReadOnlyMemory<byte>.Empty))
            .ToArray();
        Assert.Throws<InvalidOperationException>(() => AdditionalTransportNegotiation.EncodeRequest(CreateRequest(offers: tooManyOffers)));

        var largeOffer = new AdditionalTransportOffer("provider", 1, new byte[AdditionalTransportNegotiation.MaxProviderDataLength + 1]);
        Assert.Throws<InvalidOperationException>(() => AdditionalTransportNegotiation.EncodeRequest(CreateRequest(offers: largeOffer)));

        // Each offer is within its own limit, but the total exceeds the metadata limit.
        var maxOffer = new AdditionalTransportOffer("provider-a", 1, new byte[AdditionalTransportNegotiation.MaxProviderDataLength]);
        var maxOffer2 = new AdditionalTransportOffer("provider-b", 1, new byte[AdditionalTransportNegotiation.MaxProviderDataLength]);
        Assert.Throws<InvalidOperationException>(() => AdditionalTransportNegotiation.EncodeRequest(CreateRequest(offers: [maxOffer, maxOffer2])));

        Assert.Throws<InvalidOperationException>(() => AdditionalTransportNegotiation.EncodeRequest(CreateRequest(offers: new AdditionalTransportOffer("Invalid", 1, ReadOnlyMemory<byte>.Empty))));
    }

    [Fact]
    public void Response_RoundTrip()
    {
        var agreedHub = Table((-5, StreamingHubTransportMode.UnreliableSequenced));
        var agreedReceiver = Table((3, StreamingHubTransportMode.UnreliableUnordered));
        var selected = AdditionalTransportNegotiation.EncodeResponse(AdditionalTransportNegotiationResponse.Selected(42, 1, new byte[] { 7, 8 }, agreedHub, agreedReceiver));

        Assert.True(AdditionalTransportNegotiation.TryDecodeResponse(selected, out var decoded));
        Assert.Equal(AdditionalTransportNegotiationStatus.Selected, decoded!.Status);
        Assert.Equal(42, decoded.AttemptId);
        Assert.Equal(1, decoded.CandidateIndex);
        Assert.Equal(new byte[] { 7, 8 }, decoded.Selection.ToArray());
        Assert.True(decoded.AgreedClientToServerMethods.ContentEquals(agreedHub));
        Assert.True(decoded.AgreedServerToClientMethods.ContentEquals(agreedReceiver));

        Assert.True(AdditionalTransportNegotiation.TryDecodeResponse(AdditionalTransportNegotiation.EncodeResponse(AdditionalTransportNegotiationResponse.Fallback(AdditionalTransportNegotiationReason.NoAgreedMethods)), out var fallback));
        Assert.Equal(AdditionalTransportNegotiationStatus.Fallback, fallback!.Status);
        Assert.Equal(AdditionalTransportNegotiationReason.NoAgreedMethods, fallback.Reason);

        Assert.True(AdditionalTransportNegotiation.TryDecodeResponse(AdditionalTransportNegotiation.EncodeResponse(AdditionalTransportNegotiationResponse.Failed(AdditionalTransportNegotiationReason.Timeout)), out var failed));
        Assert.Equal(AdditionalTransportNegotiationStatus.Failed, failed!.Status);
        Assert.Equal(AdditionalTransportNegotiationReason.Timeout, failed.Reason);
    }

    [Fact]
    public void Response_Malformed()
    {
        var valid = AdditionalTransportNegotiation.EncodeResponse(AdditionalTransportNegotiationResponse.Selected(1, 0, new byte[] { 7 }, Table((1, StreamingHubTransportMode.UnreliableSequenced)), StreamingHubTransportModeTable.Empty));

        for (var length = 0; length < valid.Length; length++)
        {
            Assert.False(AdditionalTransportNegotiation.TryDecodeResponse(valid.AsMemory(0, length), out _));
        }
        Assert.False(AdditionalTransportNegotiation.TryDecodeResponse(valid.Concat(new byte[] { 0 }).ToArray(), out _));

        var unknownVersion = valid.ToArray();
        unknownVersion[0] = 2;
        Assert.False(AdditionalTransportNegotiation.TryDecodeResponse(unknownVersion, out _));

        // A reason at or above 128 is never sent on the wire.
        var localReason = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(localReason);
        writer.Write(AdditionalTransportNegotiation.FormatVersion);
        writer.Write((byte)AdditionalTransportNegotiationStatus.Fallback);
        writer.Write((byte)AdditionalTransportNegotiationReason.NotSupportedByServer);
        writer.Flush();
        Assert.False(AdditionalTransportNegotiation.TryDecodeResponse(localReason.WrittenMemory, out _));

        // The candidate index follows the version, the status, the reason, and the attempt identifier.
        var reader = new MessagePackReader(valid);
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadInt32();
        var invalidCandidateIndex = valid.ToArray();
        invalidCandidateIndex[(int)reader.Consumed] = AdditionalTransportNegotiation.MaxCandidates;
        Assert.False(AdditionalTransportNegotiation.TryDecodeResponse(invalidCandidateIndex, out _));
    }

    [Fact]
    public void Entries_RoundTrip_AndRejectNonCanonicalInput()
    {
        var table = Table((int.MinValue, StreamingHubTransportMode.UnreliableUnordered), (0, StreamingHubTransportMode.UnreliableSequenced), (int.MaxValue, StreamingHubTransportMode.UnreliableSequenced));

        var encoded = AdditionalTransportNegotiation.EncodeEntries(table);
        Assert.Equal(3 * AdditionalTransportNegotiation.EntryLength, encoded.Length);
        Assert.True(AdditionalTransportNegotiation.DecodeEntries(encoded).ContentEquals(table));
        Assert.Same(StreamingHubTransportModeTable.Empty, AdditionalTransportNegotiation.DecodeEntries(ReadOnlySpan<byte>.Empty));

        Assert.Throws<InvalidOperationException>(() => AdditionalTransportNegotiation.DecodeEntries(encoded.AsSpan(0, 4)));
        var unsorted = encoded.AsSpan(5, 5).ToArray().Concat(encoded.AsSpan(0, 5).ToArray()).ToArray();
        Assert.Throws<InvalidOperationException>(() => AdditionalTransportNegotiation.DecodeEntries(unsorted));
    }

    [Theory]
    [InlineData("magiconion.transport.udp", true)]
    [InlineData("a", true)]
    [InlineData("a-b_c.1", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("UDP", false)]
    [InlineData("udp transport", false)]
    [InlineData("udp/transport", false)]
    public void IsValidProviderId(string? id, bool expected)
    {
        Assert.Equal(expected, AdditionalTransportNegotiation.IsValidProviderId(id));
        Assert.False(AdditionalTransportNegotiation.IsValidProviderId(new string('a', AdditionalTransportNegotiation.MaxProviderIdLength + 1)));
        Assert.True(AdditionalTransportNegotiation.IsValidProviderId(new string('a', AdditionalTransportNegotiation.MaxProviderIdLength)));
    }

    [Fact]
    public void ControlMessage_FromClient_RoundTrip()
    {
        var buffer = new ArrayBufferWriter<byte>();
        StreamingHubMessageWriter.WriteAdditionalTransportControlMessageFromClient(buffer, AdditionalTransportControlMessageKind.Activate, 3, 5, new byte[] { 1, 2 });

        var reader = new StreamingHubServerMessageReader(buffer.WrittenMemory);
        Assert.Equal(StreamingHubMessageType.AdditionalTransportControl, reader.ReadMessageType());
        var message = reader.ReadAdditionalTransportControl();

        Assert.Equal(AdditionalTransportControlMessageKind.Activate, message.Kind);
        Assert.Equal(3, message.AttemptId);
        Assert.Equal(5, message.Code);
        Assert.Equal(new byte[] { 1, 2 }, message.Data.ToArray());
    }

    [Fact]
    public void ControlMessage_FromServer_RoundTrip()
    {
        var buffer = new ArrayBufferWriter<byte>();
        StreamingHubMessageWriter.WriteAdditionalTransportControlMessageFromServer(buffer, AdditionalTransportControlMessageKind.SelectNext, -1, 2, ReadOnlySpan<byte>.Empty);

        var reader = new StreamingHubClientMessageReader(buffer.WrittenMemory);
        Assert.Equal(StreamingHubMessageType.AdditionalTransportControl, reader.ReadMessageType());
        var message = reader.ReadAdditionalTransportControl();

        Assert.Equal(AdditionalTransportControlMessageKind.SelectNext, message.Kind);
        Assert.Equal(-1, message.AttemptId);
        Assert.Equal(2, message.Code);
        Assert.True(message.Data.IsEmpty);
    }

    [Fact]
    public void ControlMessage_FromClient_WireFormat()
    {
        var buffer = new ArrayBufferWriter<byte>();
        StreamingHubMessageWriter.WriteAdditionalTransportControlMessageFromClient(buffer, AdditionalTransportControlMessageKind.CandidateFailed, 1, 6, new byte[] { 9 });

        // Array(4)[0x7d, Kind, AttemptId, [Code, Data]]
        Assert.Equal(new byte[] { 0x94, 0x7d, 0x02, 0x01, 0x92, 0x06, 0xc4, 0x01, 0x09 }, buffer.WrittenSpan.ToArray());
    }

    [Fact]
    public void ControlMessage_FromServer_WireFormat()
    {
        var buffer = new ArrayBufferWriter<byte>();
        StreamingHubMessageWriter.WriteAdditionalTransportControlMessageFromServer(buffer, AdditionalTransportControlMessageKind.Activated, 1, 0, ReadOnlySpan<byte>.Empty);

        // Array(5)[0x7d, Kind, AttemptId, [Code, Data], Nil]
        Assert.Equal(new byte[] { 0x95, 0x7d, 0x03, 0x01, 0x92, 0x00, 0xc4, 0x00, 0xc0 }, buffer.WrittenSpan.ToArray());
    }

    [Fact]
    public void ControlMessageKinds_AreDistinctAndNonZero()
    {
        // Every control message uses the single sub-type 0x7d, and the kind follows it. The value 0 is reserved.
        var kinds = Enum.GetValues<AdditionalTransportControlMessageKind>();
        Assert.DoesNotContain(kinds, static x => (byte)x == 0);
        Assert.Equal(kinds.Length, kinds.Distinct().Count());
    }

    [Theory]
    [InlineData(0x7d, true)]
    [InlineData(0x7c, false)]
    [InlineData(0x70, false)]
    public void ControlMessage_SubType_IsRecognizedByReaders(byte subType, bool isControl)
    {
        var clientToServer = new ArrayBufferWriter<byte>();
        StreamingHubMessageWriter.WriteAdditionalTransportControlMessageFromClient(clientToServer, AdditionalTransportControlMessageKind.Activate, 1, 0, ReadOnlySpan<byte>.Empty);
        var serverToClient = new ArrayBufferWriter<byte>();
        StreamingHubMessageWriter.WriteAdditionalTransportControlMessageFromServer(serverToClient, AdditionalTransportControlMessageKind.Activated, 1, 0, ReadOnlySpan<byte>.Empty);

        // Replace the sub-type that follows the array header.
        var clientToServerBytes = clientToServer.WrittenSpan.ToArray();
        clientToServerBytes[1] = subType;
        var serverToClientBytes = serverToClient.WrittenSpan.ToArray();
        serverToClientBytes[1] = subType;

        if (isControl)
        {
            Assert.Equal(StreamingHubMessageType.AdditionalTransportControl, ReadServerSide(clientToServerBytes));
            Assert.Equal(StreamingHubMessageType.AdditionalTransportControl, ReadClientSide(serverToClientBytes));
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => ReadServerSide(clientToServerBytes));
            Assert.Throws<InvalidOperationException>(() => ReadClientSide(serverToClientBytes));
        }

        static StreamingHubMessageType ReadServerSide(ReadOnlyMemory<byte> data) => new StreamingHubServerMessageReader(data).ReadMessageType();
        static StreamingHubMessageType ReadClientSide(ReadOnlyMemory<byte> data) => new StreamingHubClientMessageReader(data).ReadMessageType();
    }

    [Fact]
    public void ControlMessage_InvalidBody_IsRejected()
    {
        // Array(4)[0x7d, Kind=Activate, AttemptId=1, [Code=0]]: the body must be [Code, Data].
        var data = new byte[] { 0x94, 0x7d, 0x01, 0x01, 0x91, 0x00 };

        Assert.Throws<InvalidOperationException>(() => Read(data));

        static void Read(ReadOnlyMemory<byte> data)
        {
            var reader = new StreamingHubServerMessageReader(data);
            reader.ReadMessageType();
            reader.ReadAdditionalTransportControl();
        }
    }
}
