using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.NetCoreServer.Hubs
{
    public interface IMessageReceiver
    {
        Task OnReceiveMessage(int senderId, string message);
    }

    public interface IChatHub : IStreamingHub<IChatHub, IMessageReceiver>
    {
        Task EchoAsync(string message);
        Task<string> EchoRetrunAsync(string message);
    }

    public class ChatHub : StreamingHubBase<IChatHub, IMessageReceiver>, IChatHub
    {
        IGroup myHoge;

        protected override ValueTask OnConnecting()
        {
            myHoge = Group.Add("HogeHoge", this.Context);
            return CompletedTask;
        }

        protected override ValueTask OnDisconnected()
        {
            myHoge.Remove(this.Context);
            return CompletedTask;
        }

        public async Task EchoAsync(string message)
        {
            Console.WriteLine("Echo here!!!");
            await BroadcastExceptSelf(myHoge).OnReceiveMessage(1230, message);
            await BroadcastExcept(myHoge, new[] { Guid.NewGuid() }).OnReceiveMessage(1230, message);


            throw new Exception("hugahuga");

        }

        public Task<string> EchoRetrunAsync(string message)
        {
            throw new Exception("foo bar");

            return Task.FromResult("foo bar:" + message);
        }
    }

    public class TestBroadcaster : IMessageReceiver
    {
        readonly IGroup group;
        readonly Guid except;
        readonly Guid[] excepts;

        public TestBroadcaster(IGroup group)
        {
            this.group = group;
        }

        public Task OnReceiveMessage(int senderId, string message)
        {
            // direct emit id:)

         //   return group.WriteExceptAsync(470021452, new DynamicArgumentTuple<int, string>(senderId, message), );

            return group.WriteAllAsync(470021452, new DynamicArgumentTuple<int, string>(senderId, message));
        }
    }
}
