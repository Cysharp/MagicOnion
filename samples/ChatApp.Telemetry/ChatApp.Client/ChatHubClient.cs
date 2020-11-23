using ChatApp.Shared.Hubs;
using ChatApp.Shared.MessagePackObjects;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ChatApp.Client
{
    public class ChatHubClient : IChatHubReceiver, IAsyncDisposable
    {
        public ChatHubClient(GrpcChannel grpcChannel, ILogger<ChatHubClient> logger)
        {
            _streamingClient = StreamingHubClient.Connect<IChatHub, IChatHubReceiver>(grpcChannel, this);
            Logger = logger;
        }

        private IChatHub _streamingClient;

        ILogger Logger { get; }


        public async Task SendMessageAsync() => await _streamingClient.SendMessageAsync("Hey there!");

        public async Task JoinAsync(Guid playerId) => await _streamingClient.JoinAsync(new JoinRequest
        {
            RoomName = "Room",
            UserName = playerId.ToString()
        });



        public void OnJoin(string name)
        {
            Logger.LogInformation("User {Username} joined", name);
        }

        public void OnLeave(string name)
        {
            Logger.LogInformation("User {Username} left room", name);
        }

        public void OnSendMessage(MessageResponse message)
        {
            Logger.LogInformation("User {Username} sent message: {Meassage}", message.UserName, message.Message);
        }

        public async ValueTask DisposeAsync() => await _streamingClient.DisposeAsync();

    }
}
