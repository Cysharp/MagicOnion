using MagicOnion;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmark.Server.Shared
{
    public interface ILongRunBenchmarkHubReciever
    {
        void OnStart(string requestType);
        void OnProcess();
        void OnEnd();
    }

    public interface ILongRunBenchmarkHub : IStreamingHub<ILongRunBenchmarkHub, ILongRunBenchmarkHubReciever>
    {
        Task Ready(string groupName, string name);
        Task Process(LongRunBenchmarkData data);
        Task End();
    }

    [MessagePackObject]
    public class LongRunBenchmarkData
    {
        [Key(0)]
        public int WaitMilliseconds { get; set; }
    }
}
