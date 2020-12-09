using Benchmark.Server.Shared;
using Benchmark.Server.Shared.Data;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.Server.Hubs
{
    public partial class BenchmarkHub : StreamingHubBase<IBenchmarkHub, IBenchmarkHubReciever>, IBenchmarkHub
    {
        public PipeWriter Writer { get; private set; }
        public PipeReader Reader { get; private set; }

        private IGroup room;

        private RequestType _requestType;
        private readonly ILogger<BenchmarkHub> _logger;

        public BenchmarkHub(ILogger<BenchmarkHub> logger)
        {
            _logger = logger;

            var pipe = new Pipe();
            Writer = pipe.Writer;
            Reader = pipe.Reader;
        }

        public async Task Ready(string groupName, string name, string requestType)
        {
            _requestType = GetRequestType(requestType);
            (room, _) = await Group.AddAsync(groupName, name);
        }

        public async Task Process(BenchmarkData data)
        {
            ProcessRequest(data);
        }

        public async Task End()
        {
            await room.RemoveAsync(Context);
        }

        protected override ValueTask OnConnecting()
        {
            Statistics.HubConnected();
            _logger.LogTrace($"{Statistics.HubConnections} New Client coming. ({Context.ContextId})");
            return CompletedTask;
        }
        protected override ValueTask OnDisconnected()
        {
            Statistics.HubDisconnected();
            _logger.LogTrace($"Client disconnected.");
            return CompletedTask;
        }

        private RequestType GetRequestType(string requestType)
        {
            if (requestType.Equals(Paths.Plaintext, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.PlainText;
            }
            return RequestType.NotRecognized;
        }

        private void ProcessRequest(BenchmarkData body)
        {
            if (_requestType == RequestType.PlainText)
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
