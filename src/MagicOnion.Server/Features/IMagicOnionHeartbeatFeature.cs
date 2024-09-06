using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Features;

/// <summary>
/// Provides StreamingHub Heartbeat features.
/// </summary>
public interface IMagicOnionHeartbeatFeature
{
    /// <summary>
    /// Gets the last received time.
    /// </summary>
    DateTimeOffset LastReceivedAt { get; }

    /// <summary>
    /// Gets the latency between client and server. Returns <see cref="TimeSpan.Zero"/> if not sent or received.
    /// </summary>
    TimeSpan Latency { get; }

    /// <summary>
    /// Gets the token to notify a time-out.
    /// </summary>
    CancellationToken TimeoutToken { get; }

    /// <summary>
    /// Sets the callback action to be performed when an Ack message is received from the client.
    /// </summary>
    /// <param name="callbackAction"></param>
    void SetAckCallback(Action<TimeSpan>? callbackAction);

    /// <summary>
    /// Unregister the current StreamingHub connection from the HeartbeatManager.
    /// </summary>
    void Unregister();
}

internal sealed class MagicOnionHeartbeatFeature(StreamingHubHeartbeatHandle handle) : IMagicOnionHeartbeatFeature
{
    public DateTimeOffset LastReceivedAt => handle.LastReceivedAt;

    public TimeSpan Latency => handle.Latency;

    public CancellationToken TimeoutToken => handle.TimeoutToken;

    public void Unregister() => handle.Unregister();

    public void SetAckCallback(Action<TimeSpan>? callbackAction) => handle.SetAckCallback(callbackAction);
}
