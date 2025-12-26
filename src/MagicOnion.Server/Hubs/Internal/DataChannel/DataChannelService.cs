using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.Hubs.Internal.DataChannel;

internal class DataChannelService(ILogger<DataChannelService> logger) : IDisposable
{
    readonly ConcurrentDictionary<ulong, ServerDataChannel> dataChannels = new();

    public ServerDataChannel CreateChannel()
    {
        var channel = new ServerDataChannel(x => RemoveChannel(x.SessionId));
        dataChannels[channel.SessionId] = channel;
        logger.LogInformation("DataChannel created. SessionId: {SessionId}", channel.SessionId);
        return channel;
    }

    void RemoveChannel(ulong sessionId)
    {
        if (dataChannels.TryRemove(sessionId, out _))
        {
            logger.LogInformation("DataChannel removed. SessionId: {SessionId}", sessionId);
        }
    }

    public bool TryGetChannel(ulong sessionId, [NotNullWhen(true)] out ServerDataChannel? channel)
    {
        return dataChannels.TryGetValue(sessionId, out channel);
    }

    public void Dispose()
    {
        foreach (var channel in dataChannels.ToArray())
        {
            channel.Value.Dispose();
            dataChannels.TryRemove(channel.Key, out _);
        }
    }
}
