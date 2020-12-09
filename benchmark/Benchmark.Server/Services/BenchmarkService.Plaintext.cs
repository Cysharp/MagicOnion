using Benchmark.Shared;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmark.Server.Services
{
    public partial class BenchmarkService : ServiceBase<IBenchmarkService>, IBenchmarkService
    {
        private static void PlainText(ref BufferWriter<WriterAdapter> writer, ReadOnlySpan<byte> body)
        {
            writer.Write(body);
        }
    }
}
