using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    internal class BoxClientStreamWriter<T> : IClientStreamWriter<T>
    {
        readonly IClientStreamWriter<Box<T>> inner;

        public BoxClientStreamWriter(IClientStreamWriter<Box<T>> inner)
        {
            this.inner = inner;
        }

        public Task WriteAsync(T message)
            => inner.WriteAsync(new Box<T>(message));

        public WriteOptions WriteOptions
        {
            get => inner.WriteOptions;
            set => inner.WriteOptions = value;
        }
        public Task CompleteAsync()
            => inner.CompleteAsync();
    }
}