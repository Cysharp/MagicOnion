using Benchmark.Server.Shared;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Server.Hubs
{
    public partial class BenchmarkHub : StreamingHubBase<IBenchmarkHub, IBenchmarkHubReciever>, IBenchmarkHub
    {
        public PipeWriter Writer { get; private set; }
        public PipeReader Reader { get; private set; }

        private readonly ILogger<BenchmarkHub> _logger;

        public BenchmarkHub(ILogger<BenchmarkHub> logger)
        {
            _logger = logger;

            var pipe = new Pipe();
            Writer = pipe.Writer;
            Reader = pipe.Reader;
        }

        public Task Process(BenchmarkData data)
        {
            ProcessRequest(data);
            return Task.CompletedTask;
        }

        protected override ValueTask OnConnecting()
        {
            Statistics.HubConnected();
            return CompletedTask;
        }
        protected override ValueTask OnDisconnected()
        {
            Statistics.HubDisconnected();
            return CompletedTask;
        }

        private void ProcessRequest(BenchmarkData body)
        {
            var writer = GetWriter(Writer, sizeHint: 160 * 16); // 160*16 is for Plaintext, for Json 160 would be enough
            PlainText(ref writer, Encoding.UTF8.GetBytes(body.PlainText).AsSpan());
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
