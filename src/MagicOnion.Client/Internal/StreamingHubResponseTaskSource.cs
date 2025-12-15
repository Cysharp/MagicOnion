using MagicOnion.Internal;
using System.Diagnostics;
using System.Threading.Tasks.Sources;

namespace MagicOnion.Client.Internal;

internal class StreamingHubResponseTaskSourcePool<T> : ObjectPool<StreamingHubResponseTaskSource<T>>
{
    public static StreamingHubResponseTaskSourcePool<T> Shared { get; } = new();

    public StreamingHubResponseTaskSourcePool()
        : base(static () => new StreamingHubResponseTaskSource<T>())
    { }

    public StreamingHubResponseTaskSource<T> RentOrCreate()
    {
        var item = RentOrCreateCore();
        item.Reset();
        return item;
    }

    public void Return(StreamingHubResponseTaskSource<T> item)
        => ReturnCore(item);
}

internal interface IStreamingHubResponseTaskSource
{
    bool TrySetException(Exception error);
    bool TrySetCanceled();
    bool TrySetCanceled(string message);
}

internal class StreamingHubResponseTaskSource<T> : IValueTaskSource<T>, IValueTaskSource, IStreamingHubResponseTaskSource
{
    ManualResetValueTaskSourceCore<T> core = new()
    {
        // NOTE: The continuations (user code) should be executed asynchronously. (Except: Unity WebGL)
        //       This is because the continuation may block the thread, for example, Console.ReadLine().
        //       If the thread is blocked, it will no longer return to the message consuming loop.
#if !UNITY_WEBGL
        RunContinuationsAsynchronously = true
#endif
    };

    public void SetResult(T result)
        => core.SetResult(result);

    public bool TrySetException(Exception error)
    {
        core.SetException(error);
        return true;
    }

    public bool TrySetCanceled()
    {
        core.SetException(new TaskCanceledException());
        return true;
    }

    public bool TrySetCanceled(string message)
    {
        core.SetException(new TaskCanceledException(message));
        return true;
    }

    public void Reset()
        => core.Reset();

    public short Version
        => core.Version;

    [DebuggerNonUserCode]
    public T GetResult(short token)
    {
        try
        {
            return core.GetResult(token);
        }
        finally
        {
            StreamingHubResponseTaskSourcePool<T>.Shared.Return(this);
        }
    }

    void IValueTaskSource.GetResult(short token)
    {
        try
        {
            core.GetResult(token);
        }
        finally
        {
            StreamingHubResponseTaskSourcePool<T>.Shared.Return(this);
        }
    }

    public ValueTaskSourceStatus GetStatus(short token)
        => core.GetStatus(token);

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        => core.OnCompleted(continuation, state, token, flags);

}
