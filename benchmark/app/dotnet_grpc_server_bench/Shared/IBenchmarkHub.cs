using MagicOnion;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmark.Server.Shared
{
    public interface IBenchmarkHubReciever
    {
        void OnProcess();
    }

    public interface IBenchmarkHub : IStreamingHub<IBenchmarkHub, IBenchmarkHubReciever>
    {
        Task Process(BenchmarkData data);
    }

    [MessagePackObject]
    public class BenchmarkData
    {
        [Key(0)]
        public string PlainText { get; set; }

        // todo: JSON? int? other benchmark data.
    }
}
