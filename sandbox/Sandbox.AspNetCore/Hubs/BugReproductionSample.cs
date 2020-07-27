using MagicOnion;
using MagicOnion.Server.Hubs;
using System.Threading.Tasks;

namespace Sandbox.AspNetCore.Hubs
{
    public interface IBugReproductionHubReceiver
    {
        void OnCall();
    }

    public interface IBugReproductionHub : IStreamingHub<IBugReproductionHub, IBugReproductionHubReceiver>
    {
        Task JoinAsync();

        Task CallAsync();
    }

    public class BugReproductionHub : StreamingHubBase<IBugReproductionHub, IBugReproductionHubReceiver>, IBugReproductionHub
    {
        IGroup room;

        public async Task JoinAsync()
        {
            const string roomName = "SampleRoom";

            this.room = await this.Group.AddAsync(roomName);
        }

        public async Task LeaveAsync()
        {
            await room.RemoveAsync(this.Context);
        }


        public async Task CallAsync()
        {
            // Reproduce the problem of delaying one frame at a time.
            for (var i = 0; i < 100; i++)
                this.Broadcast(room).OnCall();

            await Task.CompletedTask;
        }

        public IChatHub FireAndForget()
        {
            throw new System.NotImplementedException();
        }

        public Task DisposeAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task WaitForDisconnect()
        {
            throw new System.NotImplementedException();
        }
    }
}
