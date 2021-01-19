using Benchmark.Server.Shared;
using MagicOnion.Server.Hubs;
using System;
using System.Text.Encodings;

namespace Benchmark.Server.Hubs
{
    public partial class BenchmarkHub : StreamingHubBase<IBenchmarkHub, IBenchmarkHubReciever>, IBenchmarkHub
    {
        private static void PlainText(ref BufferWriter<WriterAdapter> writer, ReadOnlySpan<byte> body)
        {
            writer.Write(body);
        }
    }
}
