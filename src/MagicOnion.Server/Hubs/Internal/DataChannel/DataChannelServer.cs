using MagicOnion.Server.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace MagicOnion.Server.Hubs.Internal.DataChannel;

internal class DataChannelServer : BackgroundService
{
    readonly DataChannelService dataChannelService;
    readonly ILogger<DataChannelServer> logger;

    public DataChannelServer(DataChannelService dataChannelService, ILogger<DataChannelServer> logger)
    {
        this.dataChannelService = dataChannelService;
        this.logger = logger;
    }

    Socket CreateAndListenSocket()
    {
        var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;
        socket.DualMode = true;
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        if (OperatingSystem.IsWindows())
        {
            const uint IOC_IN = 0x80000000U;
            const uint IOC_VENDOR = 0x18000000U;
            const uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            socket.IOControl(unchecked((int)SIO_UDP_CONNRESET), [0x00], null);
        }

        socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 12345));
        return socket;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var socket = CreateAndListenSocket();
        logger.LogInformation("DataChannelServer started on UDP port 12345.");

        var buffer = GC.AllocateArray<byte>(1400, pinned: true);
        var address = new SocketAddress(AddressFamily.InterNetworkV6);

        while (!stoppingToken.IsCancellationRequested)
        {
            var readLen = await socket.ReceiveFromAsync(buffer, SocketFlags.None, address);
            var data = buffer.AsSpan(0, readLen);
            if (data.Length >= 1 + 8)
            {
                var messageType = data[0];
                var sessionId = BitConverter.ToUInt64(data.Slice(1, 8));
                if (!dataChannelService.TryGetChannel(sessionId, out var channel))
                {
                    // Invalid sessionId
                    continue;
                }

                channel.TrySetSocket(socket, address);

                //TODO: Consider handling the case where the flow of Connect->Ack->Ack is interrupted (packet drop).
                switch (messageType)
                {
                    case 0x00:
                        // Connect (Request from Client)
                        MagicOnionServerLog.DataChannelConnectRequest(logger, sessionId, channel.RemoteEndPoint?.ToString());
                        channel.SendAckFromServer();
                        break;
                    //case 0x01:
                    //    // Connect (Ack from Server)
                    //    break;
                    case 0x02:
                        // Connect (Ack from Client)
                        MagicOnionServerLog.DataChannelConnectAckReceived(logger, sessionId, channel.RemoteEndPoint?.ToString());
                        channel.SetConnected();
                        break;
                    case 0x10:
                        // Data (from Client)
                        data = data.Slice(9);
                        if (data.Length < 8 + 1)
                        {
                            // Invalid or Zero-length data packet
                            break;
                        }
                        var sequence = BitConverter.ToUInt64(data);
                        var remain = data.Slice(8);
                        MagicOnionServerLog.DataChannelDataReceived(logger, sessionId, sequence, remain.Length);
                        channel.ReceiveData(sequence, remain);
                        break;
                }
            }
            else
            {
                // Invalid packet
            }
        }
    }
}
