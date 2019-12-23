#pragma warning disable CS1998

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Server.Hubs;

namespace MagicOnion.NetCoreTests.Tests.StreamingHubBroadcastTest
{
    public class StreamingHubBroadcastTestHubReceiverMock : IStreamingHubBroadcastTestHubReceiver
    {
        public bool HasCalled { get; private set; }

        public void Call()
        {
            HasCalled = true;
        }
    }

    public interface IStreamingHubBroadcastTestHubReceiver
    {
        void Call();
    }

    public interface IStreamingHubBroadcastTestHub : IStreamingHub<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>
    {
        Task<Guid> RegisterConnectionToGroup();

        Task CallBroadcastToSelfAsync();
        Task CallBroadcastExceptSelfAsync();
        Task CallBroadcastExceptAsync(Guid connectionId);
        Task CallBroadcastExceptManyAsync(Guid[] connectionIds);
        Task CallBroadcastToAsync(Guid connectionId);
        Task CallBroadcastToManyAsync(Guid[] connectionIds);
    }

    public class StreamingHubBroadcastTestHub : StreamingHubBase<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>, IStreamingHubBroadcastTestHub
    {
        IGroup group;

        public async Task<Guid> RegisterConnectionToGroup()
        {
            this.group = await this.Group.AddAsync("Nantoka");
            return ConnectionId;
        }

        public async Task CallBroadcastToSelfAsync()
        {
            BroadcastToSelf(group).Call();
        }

        public async Task CallBroadcastExceptSelfAsync()
        {
            BroadcastExceptSelf(group).Call();
        }

        public async Task CallBroadcastExceptAsync(Guid connectionId)
        {
            BroadcastExcept(group, connectionId).Call();
        }

        public async Task CallBroadcastExceptManyAsync(Guid[] connectionIds)
        {
            BroadcastExcept(group, connectionIds).Call();
        }

        public async Task CallBroadcastToAsync(Guid connectionId)
        {
            BroadcastTo(group, connectionId).Call();
        }

        public async Task CallBroadcastToManyAsync(Guid[] connectionIds)
        {
            BroadcastTo(group, connectionIds).Call();
        }
    }
}
