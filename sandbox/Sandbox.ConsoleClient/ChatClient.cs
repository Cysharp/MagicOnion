//using Grpc.Core;
//using MagicOnion.Client;
//using Sandbox.ConsoleServer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MagicOnion;
//using MagicOnion.Server;

//namespace Sandbox.ConsoleClient
//{
//    public static class ChatClient
//    {

//        public static async Task Run(Channel channel)
//        {
//            var heartbeat = await ClientConnectionLifetimeManager.Connect(channel);

//            var client = MagicOnionClient.Create<IChatRoomService>(channel)
//                .WithHeaders(heartbeat.ToMetadata());


//            var room = await await client.CreateNewRoom("room", "hogehoge");
//            new ChatRoomStreaming(client).SubscribeAll();


//            await await client.SendMessage(room.Id, "hogehogehogehoge");

//        }

//        static void SubscribeAllListner(IChatRoomStreaming client)
//        {
//            client.OnJoin();
//            client.OnLeave();
//            client.OnMessageReceived();
//        }
//    }

//    public abstract class ChatRoomStreamingSubscriber
//    {
//        readonly IChatRoomStreaming client;

//        public ChatRoomStreamingSubscriber(IChatRoomStreaming client)
//        {
//            this.client = client;
//        }
//        public async void SubscribeAll()
//        {
//            var a = (await client.OnJoin()).ResponseStream.ForEachAsync(x => OnJoin(x));
//            var b = (await client.OnLeave()).ResponseStream.ForEachAsync(x => OnLeave(x));
//            var c = (await client.OnMessageReceived()).ResponseStream.ForEachAsync(x => OnMessageReceived(x));

//            // TODO:Client side cancellation.
//            await Task.WhenAll(a, b, c);
//        }

//        public abstract void OnJoin(RoomMember member);
//        public abstract void OnLeave(RoomMember member);
//        public abstract void OnMessageReceived(ChatMessage message);
//    }

//    public class ChatRoomStreaming : ChatRoomStreamingSubscriber
//    {
//        public ChatRoomStreaming(IChatRoomStreaming client) : base(client)
//        {
//        }

//        public override void OnJoin(RoomMember member)
//        {
//            Console.WriteLine(member);
//        }

//        public override void OnLeave(RoomMember member)
//        {
//            Console.WriteLine(member);
//        }

//        public override void OnMessageReceived(ChatMessage message)
//        {
//            Console.WriteLine(message);
//        }
//    }
//}
