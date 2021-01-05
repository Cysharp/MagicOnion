using Benchmark.Server.Shared;
using Benchmark.Server.Shared.Data;
using Benchmark.Shared;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.Server.Services
{
    public partial class BenchmarkService : ServiceBase<IBenchmarkService>, IBenchmarkService
    {
        public PipeWriter Writer { get; private set; }
        public PipeReader Reader { get; private set; }

        private readonly ILogger<BenchmarkService> _logger;

        public BenchmarkService(ILogger<BenchmarkService> logger)
        {
            _logger = logger;

            var pipe = new Pipe();
            Writer = pipe.Writer;
            Reader = pipe.Reader;
        }

        public async UnaryResult<int> SumAsync(int x, int y)
        {
            Statistics.UnaryConnected();
            try
            {
                return x + y;
            }
            catch
            {
                Statistics.UnaryError();
                return 0;
            }
            finally
            {
                Statistics.UnaryDisconnected();
            }
        }

        public async UnaryResult<Nil> PlainTextAsync(BenchmarkData data)
        {
            Statistics.UnaryConnected();
            try
            {
                //ProcessRequest(RequestType.PlainText, data);
            }
            catch
            {
                Statistics.UnaryError();
            }
            finally
            {
                Statistics.UnaryDisconnected();
            }
            return Nil.Default;
        }

        private void ProcessRequest(RequestType requestType, BenchmarkData body)
        {
            if (requestType == RequestType.PlainText)
            {
                var writer = GetWriter(Writer, sizeHint: 160 * 16); // 160*16 is for Plaintext, for Json 160 would be enough
                PlainText(ref writer, Encoding.UTF8.GetBytes(body.PlainText).AsSpan());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BufferWriter<WriterAdapter> GetWriter(PipeWriter pipeWriter, int sizeHint)
            => new BufferWriter<WriterAdapter>(new WriterAdapter(pipeWriter), sizeHint);

        private struct WriterAdapter : IBufferWriter<byte>
        {
            public PipeWriter Writer;

            public WriterAdapter(PipeWriter writer)
                => Writer = writer;

            public void Advance(int count)
                => Writer.Advance(count);

            public Memory<byte> GetMemory(int sizeHint = 0)
                => Writer.GetMemory(sizeHint);

            public Span<byte> GetSpan(int sizeHint = 0)
                => Writer.GetSpan(sizeHint);
        }
    }
}
