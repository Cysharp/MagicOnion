using Grpc.Core;

namespace MagicOnion.Internal;

internal class MagicOnionClientStreamWriter<T, TRaw> : IClientStreamWriter<T>
{
    readonly IClientStreamWriter<TRaw> inner;

    public MagicOnionClientStreamWriter(IClientStreamWriter<TRaw> inner)
    {
        this.inner = inner;
    }

    public Task WriteAsync(T message)
        => inner.WriteAsync(GrpcMethodHelper.ToRaw<T, TRaw>(message));

    public WriteOptions? WriteOptions
    {
        get => inner.WriteOptions;
        set => inner.WriteOptions = value;
    }
    public Task CompleteAsync()
        => inner.CompleteAsync();
}
