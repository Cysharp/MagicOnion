// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using ChatApp.Shared.Hubs;
using ChatApp.Shared.MessagePackObjects;
using Grpc.Net.Client;
using MagicOnion.Client;
using MessagePack;
using MessagePack.Resolvers;

if (!RuntimeFeature.IsDynamicCodeSupported)
{
    // Running on Native AOT
    StaticCompositeResolver.Instance.Register(
        BuiltinResolver.Instance,
        PrimitiveObjectResolver.Instance,
        MagicOnionGeneratedClientInitializer.Resolver,
        StandardResolver.Instance
    );
    MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions.WithResolver(StaticCompositeResolver.Instance);
}

var channel = GrpcChannel.ForAddress("http://localhost:5000");
var sessionId = Guid.NewGuid();
Console.WriteLine("Connecting...");
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, new ChatHubReceiver(sessionId));
Console.WriteLine($"Connected: {sessionId}");

Console.Write("UserName: ");
var userName = Console.ReadLine();
Console.Write("RoomName: ");
var roomName = Console.ReadLine();

Console.WriteLine($"Join: RoomName={roomName}; UserName={userName}");
await hub.JoinAsync(new JoinRequest() { RoomName = roomName, UserName = userName });
Console.WriteLine($"Joined");

while (true)
{
    var message = Console.ReadLine();
    await hub.SendMessageAsync(message);
}

[MagicOnionClientGeneration(typeof(IChatHub))]
partial class MagicOnionGeneratedClientInitializer;

class ChatHubReceiver(Guid sessionId) : IChatHubReceiver
{
    public void OnJoin(string name)
    {
        Console.WriteLine($"<Event> Join: {name}");
    }

    public void OnLeave(string name)
    {
        Console.WriteLine($"<Event> Leave: {name}");
    }

    public void OnSendMessage(MessageResponse message)
    {
        Console.WriteLine($"{message.UserName}> {message.Message}");
    }

    public async Task<string> HelloAsync(string name, int age)
    {
        Console.WriteLine("HelloAsync called");
        await Task.Delay(100);
        return $"Hello {name} ({age})!; {sessionId}";
    }
}
