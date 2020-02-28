using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;

namespace JwtAuthApp.Shared
{
    public interface ITimerHub : IStreamingHub<ITimerHub, ITimerHubReceiver>
    {
        Task SetAsync(TimeSpan interval);
    }

    public interface ITimerHubReceiver
    {
        void OnTick(string message);
    }
}
