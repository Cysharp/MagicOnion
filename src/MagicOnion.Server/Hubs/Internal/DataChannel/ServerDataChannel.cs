using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using MagicOnion.Internal;

namespace MagicOnion.Server.Hubs.Internal.DataChannel;

internal class ServerDataChannel : IDisposable
{
    readonly Channel<StreamingHubPayload> payloadChannel;
    readonly Action<ServerDataChannel> onDisposeAction;
    readonly TaskCompletionSource connectedTcs = new();

    Socket? socket;
    IPEndPoint? remoteEndPoint;
    SocketAddress? remoteAddress;
    ulong outgoingSequence = ulong.MaxValue;
    ulong incomingSequence = ulong.MaxValue;

    public ulong SessionId { get; }

    public ChannelReader<StreamingHubPayload> DataReader => payloadChannel.Reader;
    public Task Connected => connectedTcs.Task;
    public IPEndPoint? RemoteEndPoint => remoteEndPoint;

    public ServerDataChannel(Action<ServerDataChannel> onDisposeAction)
    {
        this.onDisposeAction = onDisposeAction;

        this.payloadChannel = Channel.CreateUnbounded<StreamingHubPayload>(); // TODO: Bounded?
        this.SessionId = (ulong)Random.Shared.NextInt64();
    }

    public void TrySetSocket(Socket socket, SocketAddress newRemoteAddress)
    {
        if (this.remoteAddress is null || !this.remoteAddress.Equals(newRemoteAddress))
        {
            this.socket = socket;
            this.remoteAddress = new SocketAddress(newRemoteAddress.Family);
            newRemoteAddress.Buffer.CopyTo(this.remoteAddress.Buffer);

            this.remoteEndPoint = (IPEndPoint)new IPEndPoint(IPAddress.Any, 0).Create(newRemoteAddress);
        }
    }

    public void SetConnected()
    {
        connectedTcs.TrySetResult();
    }

    [MemberNotNull(nameof(socket), nameof(remoteEndPoint), nameof(remoteAddress))]
    void EnsureAttached()
    {
        if (socket == null || remoteEndPoint == null || remoteAddress == null)
        {
            throw new InvalidOperationException("DataChannel is not attached to UdpClient.");
        }
    }

    public void ReceiveData(ulong sequence, ReadOnlySpan<byte> data)
    {
        EnsureAttached();

        var diff = sequence - incomingSequence;
        if (diff <= ulong.MaxValue / 2)
        {
            // Accept only newer sequence
            incomingSequence = sequence;
            payloadChannel.Writer.TryWrite(StreamingHubPayloadPool.Shared.RentOrCreate(data));
        }
    }

    public void SendAckFromServer()
    {
        EnsureAttached();

        var length = 1 + 8;
        var array = ArrayPool<byte>.Shared.Rent(length);
        var span = array.AsSpan(0, length);
        try
        {
            span[0] = 0x01; // Ack from Server
            BitConverter.TryWriteBytes(span.Slice(1, 8), SessionId);

            socket.SendTo(span, SocketFlags.None, remoteAddress);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    // This method is not thread-safe.
    public void SendPayload(StreamingHubPayload payload)
    {
        EnsureAttached();

        var length = 1 + 8 + 8 + payload.Length;
        var array = ArrayPool<byte>.Shared.Rent(length);
        var span = array.AsSpan(0, length);
        try
        {
            span[0] = 0x11; // Data (from Server)
            BitConverter.TryWriteBytes(span.Slice(1, 8), SessionId);
            BitConverter.TryWriteBytes(span.Slice(9, 8), outgoingSequence);
            payload.Span.CopyTo(span.Slice(17, payload.Length));

            socket.SendTo(span, SocketFlags.None, remoteAddress);
            outgoingSequence++;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    void SendClosed()
    {
        if (socket is null || remoteAddress is null) return;

        var length = 1 + 8;
        var array = ArrayPool<byte>.Shared.Rent(length);
        var span = array.AsSpan(0, length);
        try
        {
            span[0] = 0xff; // Close
            BitConverter.TryWriteBytes(span.Slice(1, 8), SessionId);
            BitConverter.TryWriteBytes(span.Slice(9, 8), outgoingSequence);

            socket.SendTo(span, SocketFlags.None, remoteAddress);
            outgoingSequence++;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public void Dispose()
    {
        payloadChannel.Writer.Complete();
        onDisposeAction(this);
        try
        {
            SendClosed();
        }
        catch
        {
            // Ignore
        }
    }
}
