using MagicOnion.Internal;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace MagicOnion.Client.Internal;

internal class ClientDataChannel : IDisposable
{
    readonly DnsEndPoint remoteEndPoint;
    readonly ulong sessionId;
    readonly CancellationTokenSource shutdownTokenSource = new();
    readonly TaskCompletionSource<bool> connectedTcs = new();
    readonly Channel<StreamingHubPayload> payloadChannel = Channel.CreateUnbounded<StreamingHubPayload>(); // TODO: unbounded/bounded?

    ulong outgoingSequence = ulong.MaxValue;
    ulong incomingSequence = ulong.MaxValue;
    Task? receiveTask;
    UdpClient? udpClient;
    IPEndPoint? resolvedRemoteEndPoint;

    static readonly TimeSpan ConnectTimeout = Debugger.IsAttached ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(1);

    public ChannelReader<StreamingHubPayload> DataReader => payloadChannel.Reader;

    public ClientDataChannel(DnsEndPoint remoteEndPoint, ulong sessionId)
    {
        this.remoteEndPoint = remoteEndPoint;
        this.sessionId = sessionId;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        using var timeoutTokenSource = new CancellationTokenSource(ConnectTimeout); // TODO
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token, shutdownTokenSource.Token);
        var timeoutTaskSource = new TaskCompletionSource<bool>();
        linkedTokenSource.Token.Register(static x =>
        {
            ((TaskCompletionSource<bool>)x!).SetCanceled();
        }, timeoutTaskSource);

        resolvedRemoteEndPoint = new IPEndPoint((await Dns.GetHostAddressesAsync(remoteEndPoint.Host)).First(), remoteEndPoint.Port); // TODO: Cancellation, Error handling on failed to resolve
        udpClient = new UdpClient(resolvedRemoteEndPoint.AddressFamily);

        // TODO: Cancellation
        SendConnect();
        receiveTask ??= RunReceiveLoop();

        var win = await Task.WhenAny(connectedTcs.Task, timeoutTaskSource.Task).ConfigureAwait(false);
        if (win == timeoutTaskSource.Task)
        {
            // timed out
            linkedTokenSource.Token.ThrowIfCancellationRequested();
        }
    }

    [MemberNotNull(nameof(udpClient))]
    void EnsureResolved()
    {
        if (udpClient == null) throw new InvalidOperationException();
    }

    void SendConnect()
    {
        EnsureResolved();

        var length = 1 + 8;
        var array = ArrayPool<byte>.Shared.Rent(length);
        var span = array.AsSpan(0, length);
        try
        {
            span[0] = 0x00; // Connect (Client->Server)
            BitConverter.GetBytes(sessionId).CopyTo(span.Slice(1));

            udpClient.Send(span.ToArray(), span.Length, resolvedRemoteEndPoint);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public void SendAckFromClient()
    {
        EnsureResolved();

        var length = 1 + 8;
        var array = ArrayPool<byte>.Shared.Rent(length);
        var span = array.AsSpan(0, length);
        try
        {
            span[0] = 0x02; // Ack (Client->Server)
            BitConverter.GetBytes(sessionId).CopyTo(span.Slice(1));

            udpClient.Send(span.ToArray(), span.Length, resolvedRemoteEndPoint);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public void ReceiveData(ulong sequence, ReadOnlySpan<byte> data)
    {
        var diff = sequence - incomingSequence;
        if (diff <= ulong.MaxValue / 2)
        {
            // Accept only newer sequence
            incomingSequence = sequence;
            payloadChannel.Writer.TryWrite(StreamingHubPayloadPool.Shared.RentOrCreate(data));
        }
    }


    // This method is not thread-safe.
    public void SendPayload(StreamingHubPayload payload)
    {
        EnsureResolved();

        var length = 1 + 8 + 8 + payload.Length;
        var array = ArrayPool<byte>.Shared.Rent(length);
        var span = array.AsSpan(0, length);
        try
        {
            span[0] = 0x10; // Data
            BitConverter.GetBytes(sessionId).CopyTo(span.Slice(1, 8));
            BitConverter.GetBytes(outgoingSequence).CopyTo(span.Slice(9, 8));
            payload.Span.CopyTo(span.Slice(17, payload.Length));

            udpClient.Send(span.ToArray(), span.Length, resolvedRemoteEndPoint);
            outgoingSequence++;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    async Task RunReceiveLoop()
    {
        EnsureResolved();

        var shutdownToken = shutdownTokenSource.Token;
        while (!shutdownToken.IsCancellationRequested)
        {
            var result = await udpClient.ReceiveAsync().ConfigureAwait(false);
            if (result.RemoteEndPoint.Equals(resolvedRemoteEndPoint))
            {
                // All messages have the message type and session ID at head
                if (result.Buffer.Length >= 1 + 8)
                {
                    var messageType = result.Buffer[0];
                    var sessionId = BitConverter.ToUInt64(result.Buffer, 1);
                    if (sessionId != this.sessionId) continue;

                    switch (messageType)
                    {
                        case 0x01: // Connect:Ack (Server -> Client)
                            SendAckFromClient();
                            connectedTcs.TrySetResult(true);
                            break;
                        case 0x03: // Data
                            if (result.Buffer.Length > 1 + 8 + 8 /* Sequence */)
                            {
                                var sequence = BitConverter.ToUInt64(result.Buffer, 1 + 8);
                                ReceiveData(sequence, result.Buffer.AsSpan(17));
                            }
                            break;
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        shutdownTokenSource.Cancel();
        udpClient?.Dispose();

        if (receiveTask is not null)
        {
            try
            {
                receiveTask.GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore
            }
        }
    }
}
