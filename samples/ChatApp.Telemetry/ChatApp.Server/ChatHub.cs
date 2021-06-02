using ChatApp.Shared.Hubs;
using ChatApp.Shared.MessagePackObjects;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ChatApp.Server
{
    /// <summary>
    /// Chat server processing.
    /// One class instance for one connection.
    /// </summary>
    public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
    {
        private IGroup room;
        private string myName;
        private readonly ActivitySource mysqlActivity = BackendActivitySources.MySQLActivitySource;
        private readonly ActivitySource redisActivity = BackendActivitySources.RedisActivitySource;
        private readonly MagicOnionOpenTelemetryOptions options;

        public ChatHub(MagicOnionOpenTelemetryOptions options)
        {
            this.options = options;
        }

        public async Task JoinAsync(JoinRequest request)
        {
            room = await this.Group.AddAsync(request.RoomName);
            myName = request.UserName;
            Broadcast(this.room).OnJoin(request.UserName);

            // dummy external operation db.
            var random = new Random();
            using (var activity = mysqlActivity.StartActivity("room/insert", ActivityKind.Internal))
            {
                // this is sample. use orm or any safe way.
                activity?.SetTag("service.name", options.ServiceName);
                activity?.SetTag("table", "rooms");
                activity?.SetTag("query", $"INSERT INTO rooms VALUES (0, '@room', '@username', '1');");
                activity?.SetTag("parameter.room", request.RoomName);
                activity?.SetTag("parameter.username", request.UserName);
                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(2, 20)));
            }
            using (var activity = redisActivity.StartActivity($"member/status", ActivityKind.Internal))
            {
                activity?.SetTag("service.name", options.ServiceName);
                activity?.SetTag("command", "set");
                activity?.SetTag("parameter.key", this.myName);
                activity?.SetTag("parameter.value", "1");
                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(1, 5)));
            }

            // add this time only tag
            var scope = this.Context.GetTraceScope(nameof(IChatHub) + "/" + nameof(JoinAsync));
            scope?.SetTags(new Dictionary<string, string> { { "my_key", Context.ContextId.ToString() } });
        }

        public async Task LeaveAsync()
        {
            await this.room.RemoveAsync(this.Context);
            this.Broadcast(this.room).OnLeave(this.myName);

            // dummy external operation.
            var random = new Random();
            using (var activity = mysqlActivity.StartActivity("room/update", ActivityKind.Internal))
            {
                // this is sample. use orm or any safe way.
                activity?.SetTag("service.name", options.ServiceName);
                activity?.SetTag("table", "rooms");
                activity?.SetTag("query", $"UPDATE rooms SET status=0 WHERE id='room' AND name='@username';");
                activity?.SetTag("parameter.room", this.room.GroupName);
                activity?.SetTag("parameter.username", this.myName);
                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(2, 20)));
            }

            using (var activity = redisActivity.StartActivity($"member/status", ActivityKind.Internal))
            {
                activity?.SetTag("service.name", options.ServiceName);
                activity?.SetTag("command", "set");
                activity?.SetTag("parameter.key", this.myName);
                activity?.SetTag("parameter.value", "0");
                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(1, 5)));
            }
        }

        public async Task SendMessageAsync(string message)
        {
            var response = new MessageResponse { UserName = this.myName, Message = message };
            this.Broadcast(this.room).OnSendMessage(response);

            // dummy external operation.
            var random = new Random();
            using (var activity = redisActivity.StartActivity($"chat_latest_message", ActivityKind.Internal))
            {
                activity?.SetTag("service.name", options.ServiceName);
                activity?.SetTag("command", "set");
                activity?.SetTag("parameter.key", room.GroupName);
                activity?.SetTag("parameter.value", $"{myName}={message}");
                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(1, 5)));
            }

            await Task.CompletedTask;
        }

        public async Task GenerateException(string message)
        {
            var ex = new Exception(message);

            // dummy external operation.
            var random = new Random();
            using (var activity = mysqlActivity.StartActivity("errors/insert", ActivityKind.Internal))
            {
                // this is sample. use orm or any safe way.
                activity?.SetTag("service.name", options.ServiceName);
                activity?.SetTag("table", "errors");
                activity?.SetTag("query", $"INSERT INTO rooms VALUES ('{ex.Message}', '{ex.StackTrace}');");
                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(2, 20)));
            }
            throw ex;
        }

        // It is not called because it is a method as a sample of arguments.
        public Task SampleMethod(List<int> sampleList, Dictionary<int, string> sampleDictionary)
        {
            throw new System.NotImplementedException();
        }

        protected override ValueTask OnConnecting()
        {
            // use hub trace context to set your span on same level. Otherwise parent will automatically set.
            var scope = this.Context.GetTraceScope();
            scope?.SetTags(new Dictionary<string, string> { { "magiconion.connect_status", "connected" } });

            // handle connection if needed.
            Console.WriteLine($"client connected {this.Context.ContextId}");
            return CompletedTask;
        }

        protected override ValueTask OnDisconnected()
        {
            // use hub trace context to set your span on same level. Otherwise parent will automatically set.
            var scope = this.Context.GetTraceScope();
            scope?.SetTags(new Dictionary<string, string> { { "magiconion.connect_status", "disconnected" } });

            // handle disconnection if needed.
            // on disconnecting, if automatically removed this connection from group.
            return CompletedTask;
        }
    }
}
