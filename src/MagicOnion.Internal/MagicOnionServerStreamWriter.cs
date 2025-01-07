using Grpc.Core;

namespace MagicOnion.Internal
{
    internal class MagicOnionServerStreamWriter<T, TRaw> : IServerStreamWriter<T>
    {
        readonly IServerStreamWriter<TRaw> inner;

        public MagicOnionServerStreamWriter(IServerStreamWriter<TRaw> inner)
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
    }
}
