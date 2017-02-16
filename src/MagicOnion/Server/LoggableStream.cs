using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    internal class LoggableStreamWriter<T> : IAsyncStreamWriter<T>
    {
        readonly IMagicOnionLogger logger;
        readonly ServiceContext context;
        readonly IAsyncStreamWriter<T> writer;

        public LoggableStreamWriter(IMagicOnionLogger logger, ServiceContext context, IAsyncStreamWriter<T> writer)
        {
            this.logger = logger;
            this.context = context;
            this.writer = writer;
        }

        public WriteOptions WriteOptions
        {
            get
            {
                return writer.WriteOptions;
            }

            set
            {
                writer.WriteOptions = value;
            }
        }

        public Task WriteAsync(T message)
        {
            logger.WriteToStream(context);
            return writer.WriteAsync(message);
        }
    }

    internal class LoggableStreamReader<T> : IAsyncStreamReader<T>
    {
        readonly IMagicOnionLogger logger;
        readonly ServiceContext context;
        readonly IAsyncStreamReader<T> reader;

        public LoggableStreamReader(IMagicOnionLogger logger, ServiceContext context, IAsyncStreamReader<T> reader)
        {
            this.logger = logger;
            this.context = context;
            this.reader = reader;
        }

        public T Current
        {
            get
            {
                return reader.Current;
            }
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            var result = await reader.MoveNext(cancellationToken).ConfigureAwait(false);
            logger.ReadFromStream(context, !result);
            return result;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }

}
