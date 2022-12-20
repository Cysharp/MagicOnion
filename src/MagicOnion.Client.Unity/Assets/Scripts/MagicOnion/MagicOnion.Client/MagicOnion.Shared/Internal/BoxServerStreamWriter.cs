using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    internal static class BoxServerStreamWriter
    {
        public static IServerStreamWriter<T> Create<T, TRaw>(IServerStreamWriter<TRaw> rawStreamWriter)
            => (typeof(TRaw) == typeof(Box<T>)) ? new BoxServerStreamWriter<T>((IServerStreamWriter<Box<T>>)rawStreamWriter) : (IServerStreamWriter<T>)rawStreamWriter;
    }

    internal class BoxServerStreamWriter<T> : IServerStreamWriter<T>
    {
        readonly IServerStreamWriter<Box<T>> inner;

        public BoxServerStreamWriter(IServerStreamWriter<Box<T>> inner)
        {
            this.inner = inner;
        }

        public Task WriteAsync(T message)
            => inner.WriteAsync(Box.Create(message));

        public WriteOptions WriteOptions
        {
            get => inner.WriteOptions;
            set => inner.WriteOptions = value;
        }
    }
}
