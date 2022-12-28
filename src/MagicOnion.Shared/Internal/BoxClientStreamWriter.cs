using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    internal static class BoxClientStreamWriter
    {
        public static IClientStreamWriter<T> Create<T, TRaw>(IClientStreamWriter<TRaw> rawStreamWriter)
            => (typeof(TRaw) == typeof(Box<T>)) ? new BoxClientStreamWriter<T>((IClientStreamWriter<Box<T>>)rawStreamWriter) : (IClientStreamWriter<T>)rawStreamWriter;
    }

    internal class BoxClientStreamWriter<T> : IClientStreamWriter<T>
    {
        readonly IClientStreamWriter<Box<T>> inner;

        public BoxClientStreamWriter(IClientStreamWriter<Box<T>> inner)
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
        public Task CompleteAsync()
            => inner.CompleteAsync();
    }
}
