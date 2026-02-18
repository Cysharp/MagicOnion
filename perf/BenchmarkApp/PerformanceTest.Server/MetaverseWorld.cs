using System.Collections.Concurrent;
using PerformanceTest.Shared;

namespace PerformanceTest.Server;

public class MetaverseWorld : IDisposable
{
    readonly ConcurrentDictionary<Guid, ClientState> clients = new();
    PeriodicTimer? broadcastTimer;
    Task? broadcastTask;
    CancellationTokenSource? cancellationTokenSource;
    
    long frameNumber;

    public int ClientCount => clients.Count;
    public long CurrentFrame => Interlocked.Read(ref frameNumber);

    public void AddClient(Guid clientId)
    {
        clients.TryAdd(clientId, new ClientState
        {
            ClientId = clientId,
            Position = Shared.Vector3.Zero,
            LastUpdateTicks = 0
        });
    }

    public void RemoveClient(Guid clientId)
    {
        clients.TryRemove(clientId, out _);
    }

    public void UpdateClientPosition(Guid clientId, Shared.Vector3 position)
    {
        if (clients.TryGetValue(clientId, out var state))
        {
            state.Position = position;
            state.LastUpdateTicks = Environment.TickCount64;
        }
    }

    public void WriteAllClientPositions(Span<BroadcastPositionMessage> span)
    {
        var index = 0;
        foreach (var kvp in clients)
        {
            span[index++] = new BroadcastPositionMessage(
                (int)kvp.Key.GetHashCode(),
                kvp.Value.Position
            );
        }
    }

    public void StartBroadcast(int fps, Action broadcastAction)
    {
        if (broadcastTimer is not null)
        {
            StopBroadcast();
        }

        var interval = TimeSpan.FromMilliseconds(1000.0 / fps);
        broadcastTimer = new PeriodicTimer(interval);
        cancellationTokenSource = new CancellationTokenSource();
        
        broadcastTask = Task.Run(async () =>
        {
            try
            {
                while (await broadcastTimer.WaitForNextTickAsync(cancellationTokenSource.Token))
                {
                    try
                    {
                        broadcastAction();
                        Interlocked.Increment(ref frameNumber);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MetaverseWorld] Broadcast error: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }

            Console.WriteLine("[MetaverseWorld] Broadcast stopped.");
        }, cancellationTokenSource.Token);
    }

    public void StopBroadcast()
    {
        cancellationTokenSource?.Cancel();
        
        try
        {
            broadcastTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Ignore cancellation exceptions
        }
        
        cancellationTokenSource?.Dispose();
        broadcastTimer?.Dispose();
        
        cancellationTokenSource = null;
        broadcastTimer = null;
        broadcastTask = null;
        
        Interlocked.Exchange(ref frameNumber, 0);
    }

    public void Dispose()
    {
        StopBroadcast();
        clients.Clear();
    }
}

class ClientState
{
    public Guid ClientId { get; set; }
    public Shared.Vector3 Position { get; set; }
    public long LastUpdateTicks { get; set; }
}
