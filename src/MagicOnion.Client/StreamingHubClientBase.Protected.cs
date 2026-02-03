using Grpc.Core;
using MagicOnion.Client.Internal;
using System.Buffers;

namespace MagicOnion.Client;

partial class StreamingHubClientBase<TStreamingHub, TReceiver>
{
    // Helper methods to make building clients easy.
    protected void SetResultForResponse<TResponse>(object taskSource, ReadOnlyMemory<byte> data)
        => ((StreamingHubResponseTaskSource<TResponse>)taskSource).SetResult(Deserialize<TResponse>(data));
    protected void Serialize<T>(IBufferWriter<byte> writer, in T value)
        => messageSerializer.Serialize<T>(writer, value);
    protected T Deserialize<T>(ReadOnlyMemory<byte> data)
        => messageSerializer.Deserialize<T>(new ReadOnlySequence<byte>(data));

    protected abstract void OnClientResultEvent(int methodId, Guid messageId, ReadOnlyMemory<byte> data);
    protected abstract void OnResponseEvent(int methodId, object taskSource, ReadOnlyMemory<byte> data);
    protected abstract void OnBroadcastEvent(int methodId, ReadOnlyMemory<byte> data);

    #region for API binary backward compatibility
    protected ValueTask<TResponse> WriteMessageFireAndForgetValueTaskOfTAsync<TRequest, TResponse>(int methodId, TRequest message)
        => WriteMessageFireAndForgetValueTaskOfTAsync<TRequest, TResponse>(methodId, message, TransportReliability.Reliable);

    protected Task<TResponse> WriteMessageFireAndForgetTaskAsync<TRequest, TResponse>(int methodId, TRequest message)
        => WriteMessageFireAndForgetTaskAsync<TRequest, TResponse>(methodId, message, TransportReliability.Reliable);

    protected ValueTask WriteMessageFireAndForgetValueTaskAsync<TRequest, TResponse>(int methodId, TRequest message)
        => WriteMessageFireAndForgetValueTaskAsync<TRequest, TResponse>(methodId, message, TransportReliability.Reliable);
    #endregion

    protected Task<TResponse> WriteMessageFireAndForgetTaskAsync<TRequest, TResponse>(int methodId, TRequest message, TransportReliability reliability)
        => WriteMessageFireAndForgetValueTaskOfTAsync<TRequest, TResponse>(methodId, message).AsTask();

    protected ValueTask<TResponse> WriteMessageFireAndForgetValueTaskOfTAsync<TRequest, TResponse>(int methodId, TRequest message, TransportReliability reliability)
    {
        ThrowIfDisposed();
        ThrowIfDisconnected();

        var v = BuildRequestMessage(methodId, message);
        if (reliability == TransportReliability.Unreliable && dataChannel is not null)
        {
            dataChannel.SendPayload(v);
        }
        else
        {
            _ = writerQueue.Writer.TryWrite(v);
        }

        return default;
    }

    protected ValueTask WriteMessageFireAndForgetValueTaskAsync<TRequest, TResponse>(int methodId, TRequest message, TransportReliability reliability)
    {
        WriteMessageFireAndForgetValueTaskOfTAsync<TRequest, TResponse>(methodId, message, reliability);
        return default;
    }

    protected Task<TResponse> WriteMessageWithResponseTaskAsync<TRequest, TResponse>(int methodId, TRequest message)
        => WriteMessageWithResponseValueTaskOfTAsync<TRequest, TResponse>(methodId, message).AsTask();

    protected ValueTask<TResponse> WriteMessageWithResponseValueTaskOfTAsync<TRequest, TResponse>(int methodId, TRequest message)
    {
        ThrowIfDisposed();
        ThrowIfDisconnected();

        var mid = Interlocked.Increment(ref messageIdSequence);

        var taskSource = StreamingHubResponseTaskSourcePool<TResponse>.Shared.RentOrCreate();
        lock (responseFutures)
        {
            responseFutures[mid] = taskSource;
        }

        var v = BuildRequestMessage(methodId, mid, message);
        if (!writerQueue.Writer.TryWrite(v))
        {
            // If the channel writer is closed, it is likely that the connection has already been disconnected.
            ThrowIfDisconnected();
        }

        return new ValueTask<TResponse>(taskSource, taskSource.Version); // wait until server return response(or error). if connection was closed, throws cancellation from DisposeAsyncCore.
    }

    protected ValueTask WriteMessageWithResponseValueTaskAsync<TRequest, TResponse>(int methodId, TRequest message)
    {
        ThrowIfDisposed();
        ThrowIfDisconnected();

        var mid = Interlocked.Increment(ref messageIdSequence);

        var taskSource = StreamingHubResponseTaskSourcePool<TResponse>.Shared.RentOrCreate();
        lock (responseFutures)
        {
            responseFutures[mid] = taskSource;
        }

        var v = BuildRequestMessage(methodId, mid, message);
        if (!writerQueue.Writer.TryWrite(v))
        {
            // If the channel writer is closed, it is likely that the connection has already been disconnected.
            ThrowIfDisconnected();
        }

        return new ValueTask(taskSource, taskSource.Version); // wait until server return response(or error). if connection was closed, throws cancellation from DisposeAsyncCore.
    }

    protected void AwaitAndWriteClientResultResponseMessage(int methodId, Guid clientResultMessageId, Task task)
        => AwaitAndWriteClientResultResponseMessage(methodId, clientResultMessageId, new ValueTask(task));

    protected async void AwaitAndWriteClientResultResponseMessage(int methodId, Guid clientResultMessageId, ValueTask task)
    {
        try
        {
            await task.ConfigureAwait(false);
            await WriteClientResultResponseMessageAsync(methodId, clientResultMessageId, MessagePack.Nil.Default).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await WriteClientResultResponseMessageForErrorAsync(methodId, clientResultMessageId, e).ConfigureAwait(false);
        }
    }

    protected void AwaitAndWriteClientResultResponseMessage<T>(int methodId, Guid clientResultMessageId, Task<T> task)
        => AwaitAndWriteClientResultResponseMessage(methodId, clientResultMessageId, new ValueTask<T>(task));

    protected async void AwaitAndWriteClientResultResponseMessage<T>(int methodId, Guid clientResultMessageId, ValueTask<T> task)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            await WriteClientResultResponseMessageAsync(methodId, clientResultMessageId, result).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await WriteClientResultResponseMessageForErrorAsync(methodId, clientResultMessageId, e).ConfigureAwait(false);
        }
    }

    protected async void WriteClientResultResponseMessageForError(int methodId, Guid clientResultMessageId, Exception ex)
    {
        try
        {
            await WriteClientResultResponseMessageForErrorAsync(methodId, clientResultMessageId, ex).ConfigureAwait(false);
        }
        catch
        {
            // Ignore Exception
        }
    }

    protected Task WriteClientResultResponseMessageAsync<T>(int methodId, Guid clientResultMessageId, T result)
    {
        var v = BuildClientResultResponseMessage(methodId, clientResultMessageId, result);
        _ = writerQueue.Writer.TryWrite(v);
        return Task.CompletedTask;
    }

    protected Task WriteClientResultResponseMessageForErrorAsync(int methodId, Guid clientResultMessageId, Exception ex)
    {
        var statusCode = ex is RpcException rpcException
            ? rpcException.StatusCode
            : StatusCode.Internal;

        var v = BuildClientResultResponseMessageForError(methodId, clientResultMessageId, (int)statusCode, ex.Message, ex);
        _ = writerQueue.Writer.TryWrite(v);

        return Task.CompletedTask;
    }
}
