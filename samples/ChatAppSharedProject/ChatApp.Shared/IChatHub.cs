using MagicOnion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Shared
{
    /// <summary>
    /// Client -> Server API (Streaming)
    /// </summary>
    public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
    {
        Task JoinAsync(JoinRequest request);

        Task LeaveAsync();

        Task SendMessageAsync(string message);

        // It is not called because it is a method as a sample of arguments.
        Task SampleMethod(List<int> sampleList, Dictionary<int, string> sampleDictionary);
    }
}
