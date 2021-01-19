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
        void OnStart(string requestType);
        void OnProcess();
        void OnEnd();
    }

    public interface IBenchmarkHub : IStreamingHub<IBenchmarkHub, IBenchmarkHubReciever>
    {
        Task Ready(string groupName, string name, string requestType);
        Task Process(BenchmarkData data);
        Task End();
    }

    [MessagePackObject]
    public class BenchmarkData
    {
        [Key(0)]
        public string PlainText { get; set; }

        // todo: JSON? int? other benchmark data.
    }
}
