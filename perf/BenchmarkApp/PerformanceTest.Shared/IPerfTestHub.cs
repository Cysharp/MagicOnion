using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;

namespace PerformanceTest.Shared
{
    public interface IPerfTestHub : IStreamingHub<IPerfTestHub, IPerfTestHubReceiver>
    {
        Task<int> CallMethodAsync(string arg1, int arg2, int arg3, int arg4);
        ValueTask<int> CallMethodValueTaskAsync(string arg1, int arg2, int arg3, int arg4);
        Task<ComplexResponse> CallMethodComplexAsync(string arg1, int arg2, int arg3, int arg4);
        ValueTask<ComplexResponse> CallMethodComplexValueTaskAsync(string arg1, int arg2, int arg3, int arg4);
        Task<(int StatusCode, byte[] Data)> CallMethodLargePayloadAsync(string arg1, int arg2, int arg3, int arg4, byte[] arg5);
    }

    public interface IPerfTestHubReceiver
    {}
}
