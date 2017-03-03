#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
namespace MagicOnion
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::MagicOnion;
    using global::MagicOnion.Client;

    public static partial class MagicOnionInitializer
    {
        static bool isRegistered = false;

        public static void Register()
        {
            if(isRegistered) return;
            isRegistered = true;

            MagicOnionClientRegistry<Sandbox.ConsoleServer.IArgumentPattern>.Register((x, y) => new Sandbox.ConsoleServer.IArgumentPatternClient(x, y));
            MagicOnionClientRegistry<Sandbox.ConsoleServer.IChatRoomService>.Register((x, y) => new Sandbox.ConsoleServer.IChatRoomServiceClient(x, y));
            MagicOnionClientRegistry<Sandbox.ConsoleServer.IHeartbeat>.Register((x, y) => new Sandbox.ConsoleServer.IHeartbeatClient(x, y));
            MagicOnionClientRegistry<Sandbox.ConsoleServer.ISendMetadata>.Register((x, y) => new Sandbox.ConsoleServer.ISendMetadataClient(x, y));
            MagicOnionClientRegistry<Sandbox.ConsoleServer.IStandard>.Register((x, y) => new Sandbox.ConsoleServer.IStandardClient(x, y));
            MagicOnionClientRegistry<Sandbox.ConsoleServer.IMyFirstService>.Register((x, y) => new Sandbox.ConsoleServer.IMyFirstServiceClient(x, y));
        }
    }
}
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Sandbox.ConsoleServer {
    using MagicOnion;
    using MagicOnion.Client;
    using UniRx;
    using Grpc.Core;
    using MessagePack;



    public interface IArgumentPattern : MagicOnion.IService<Sandbox.ConsoleServer.IArgumentPattern>
    {
   
        UnaryResult<global::SharedLibrary.MyHugeResponse> Unary1(int x, int y, string z = "unknown", global::SharedLibrary.MyEnum e = SharedLibrary.MyEnum.Orange, global::SharedLibrary.MyStructResponse soho = default(global::SharedLibrary.MyStructResponse), ulong zzz = 9, global::SharedLibrary.MyRequest req = null);
   
        UnaryResult<global::SharedLibrary.MyResponse> Unary2(global::SharedLibrary.MyRequest req);
   
        UnaryResult<global::SharedLibrary.MyResponse> Unary3();
   
        UnaryResult<global::SharedLibrary.MyStructResponse> Unary5(global::SharedLibrary.MyStructRequest req);
   
        ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult1(int x, int y, string z = "unknown");
   
        ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult2(global::SharedLibrary.MyRequest req);
   
        ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult3();
   
        ServerStreamingResult<global::MessagePack.Nil> ServerStreamingResult4();
   
        ServerStreamingResult<global::SharedLibrary.MyStructResponse> ServerStreamingResult5(global::SharedLibrary.MyStructRequest req);
   
        UnaryResult<bool> UnaryS1(global::System.DateTime dt, global::System.DateTimeOffset dt2);
   
        UnaryResult<bool> UnaryS2(int[] arrayPattern);
   
        UnaryResult<bool> UnaryS3(int[] arrayPattern1, string[] arrayPattern2, global::SharedLibrary.MyEnum[] arrayPattern3);
    }




    public interface IChatRoomService : MagicOnion.IService<Sandbox.ConsoleServer.IChatRoomService>, Sandbox.ConsoleServer.IChatRoomCommand, Sandbox.ConsoleServer.IChatRoomStreaming
    {
    }




    public interface IHeartbeat : MagicOnion.IService<Sandbox.ConsoleServer.IHeartbeat>
    {
   
        DuplexStreamingResult<global::MessagePack.Nil, global::MessagePack.Nil> Connect();
   
        UnaryResult<global::MessagePack.Nil> TestSend(string connectionId);
    }




    public interface ISendMetadata : MagicOnion.IService<Sandbox.ConsoleServer.ISendMetadata>
    {
   
        UnaryResult<int> PangPong();
    }




    public interface IStandard : MagicOnion.IService<Sandbox.ConsoleServer.IStandard>
    {
   
        UnaryResult<int> Unary1Async(int x, int y);
   
        ClientStreamingResult<int, string> ClientStreaming1Async();
   
        ServerStreamingResult<int> ServerStreamingAsync(int x, int y, int z);
   
        DuplexStreamingResult<int, int> DuplexStreamingAsync();
    }




    public interface IMyFirstService : MagicOnion.IService<Sandbox.ConsoleServer.IMyFirstService>
    {
   
        UnaryResult<string> SumAsync(int x, int y);
   
        UnaryResult<string> SumAsync2(int x, int y);
   
        ClientStreamingResult<int, string> StreamingOne();
   
        ServerStreamingResult<string> StreamingTwo(int x, int y, int z);
   
        ServerStreamingResult<string> StreamingTwo2(int x, int y, int z = 9999);
   
        DuplexStreamingResult<int, string> StreamingThree();
    }




    public interface IChatRoomCommand
    {
   
        UnaryResult<global::Sandbox.ChatRoomResponse> CreateNewRoom(string roomName, string nickName);
   
        UnaryResult<global::Sandbox.ChatRoomResponse> Join(string roomId, string nickName);
   
        UnaryResult<global::Sandbox.ChatRoomResponse[]> GetRooms();
   
        UnaryResult<bool> Leave(string roomId);
   
        UnaryResult<bool> SendMessage(string roomId, string message);
    }




    public interface IChatRoomStreaming
    {
   
        ServerStreamingResult<global::Sandbox.RoomMember> OnJoin();
   
        ServerStreamingResult<global::Sandbox.RoomMember> OnLeave();
   
        ServerStreamingResult<global::Sandbox.ChatMessage> OnMessageReceived();
    }




    public class IArgumentPatternClient : MagicOnionClientBase<IArgumentPattern>, IArgumentPattern
    {
        static readonly Method<byte[], byte[]> Unary1Method;
        static readonly Method<byte[], byte[]> Unary2Method;
        static readonly Method<byte[], byte[]> Unary3Method;
        static readonly Method<byte[], byte[]> Unary5Method;
        static readonly Method<byte[], byte[]> ServerStreamingResult1Method;
        static readonly Method<byte[], byte[]> ServerStreamingResult2Method;
        static readonly Method<byte[], byte[]> ServerStreamingResult3Method;
        static readonly Method<byte[], byte[]> ServerStreamingResult4Method;
        static readonly Method<byte[], byte[]> ServerStreamingResult5Method;
        static readonly Method<byte[], byte[]> UnaryS1Method;
        static readonly Method<byte[], byte[]> UnaryS2Method;
        static readonly Method<byte[], byte[]> UnaryS3Method;

        static IArgumentPatternClient()
        {
            Unary1Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "Unary1", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            Unary2Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "Unary2", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            Unary3Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "Unary3", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            Unary5Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "Unary5", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ServerStreamingResult1Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult1", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ServerStreamingResult2Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult2", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ServerStreamingResult3Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult3", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ServerStreamingResult4Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult4", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ServerStreamingResult5Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IArgumentPattern", "ServerStreamingResult5", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            UnaryS1Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "UnaryS1", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            UnaryS2Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "UnaryS2", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            UnaryS3Method = new Method<byte[], byte[]>(MethodType.Unary, "IArgumentPattern", "UnaryS3", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        IArgumentPatternClient()
        {
        }

        public IArgumentPatternClient(CallInvoker callInvoker, IFormatterResolver resolver)
            : base(callInvoker, resolver)
        {
        }

        protected override MagicOnionClientBase<IArgumentPattern> Clone()
        {
            var clone = new IArgumentPatternClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            return clone;
        }
   
        public UnaryResult<global::SharedLibrary.MyHugeResponse> Unary1(int x, int y, string z = "unknown", global::SharedLibrary.MyEnum e = SharedLibrary.MyEnum.Orange, global::SharedLibrary.MyStructResponse soho = default(global::SharedLibrary.MyStructResponse), ulong zzz = 9, global::SharedLibrary.MyRequest req = null)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, string, global::SharedLibrary.MyEnum, global::SharedLibrary.MyStructResponse, ulong, global::SharedLibrary.MyRequest>(x, y, z, e, soho, zzz, req), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(Unary1Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyHugeResponse>(__callResult, base.resolver);
        }

        public UnaryResult<global::SharedLibrary.MyResponse> Unary2(global::SharedLibrary.MyRequest req)
        {
            var __request = MessagePackSerializer.Serialize(req, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(Unary2Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyResponse>(__callResult, base.resolver);
        }

        public UnaryResult<global::SharedLibrary.MyResponse> Unary3()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncUnaryCall(Unary3Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyResponse>(__callResult, base.resolver);
        }

        public UnaryResult<global::SharedLibrary.MyStructResponse> Unary5(global::SharedLibrary.MyStructRequest req)
        {
            var __request = MessagePackSerializer.Serialize(req, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(Unary5Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyStructResponse>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult1(int x, int y, string z = "unknown")
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, string>(x, y, z), base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult1Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyResponse>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult2(global::SharedLibrary.MyRequest req)
        {
            var __request = MessagePackSerializer.Serialize(req, base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult2Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyResponse>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult3()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult3Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyResponse>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::MessagePack.Nil> ServerStreamingResult4()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult4Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::MessagePack.Nil>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::SharedLibrary.MyStructResponse> ServerStreamingResult5(global::SharedLibrary.MyStructRequest req)
        {
            var __request = MessagePackSerializer.Serialize(req, base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult5Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyStructResponse>(__callResult, base.resolver);
        }

        public UnaryResult<bool> UnaryS1(global::System.DateTime dt, global::System.DateTimeOffset dt2)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<global::System.DateTime, global::System.DateTimeOffset>(dt, dt2), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(UnaryS1Method, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, base.resolver);
        }

        public UnaryResult<bool> UnaryS2(int[] arrayPattern)
        {
            var __request = MessagePackSerializer.Serialize(arrayPattern, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(UnaryS2Method, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, base.resolver);
        }

        public UnaryResult<bool> UnaryS3(int[] arrayPattern1, string[] arrayPattern2, global::SharedLibrary.MyEnum[] arrayPattern3)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int[], string[], global::SharedLibrary.MyEnum[]>(arrayPattern1, arrayPattern2, arrayPattern3), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(UnaryS3Method, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, base.resolver);
        }

    }


    public class IChatRoomServiceClient : MagicOnionClientBase<IChatRoomService>, IChatRoomService
    {
        static readonly Method<byte[], byte[]> CreateNewRoomMethod;
        static readonly Method<byte[], byte[]> JoinMethod;
        static readonly Method<byte[], byte[]> GetRoomsMethod;
        static readonly Method<byte[], byte[]> LeaveMethod;
        static readonly Method<byte[], byte[]> SendMessageMethod;
        static readonly Method<byte[], byte[]> OnJoinMethod;
        static readonly Method<byte[], byte[]> OnLeaveMethod;
        static readonly Method<byte[], byte[]> OnMessageReceivedMethod;

        static IChatRoomServiceClient()
        {
            CreateNewRoomMethod = new Method<byte[], byte[]>(MethodType.Unary, "IChatRoomService", "CreateNewRoom", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            JoinMethod = new Method<byte[], byte[]>(MethodType.Unary, "IChatRoomService", "Join", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            GetRoomsMethod = new Method<byte[], byte[]>(MethodType.Unary, "IChatRoomService", "GetRooms", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            LeaveMethod = new Method<byte[], byte[]>(MethodType.Unary, "IChatRoomService", "Leave", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            SendMessageMethod = new Method<byte[], byte[]>(MethodType.Unary, "IChatRoomService", "SendMessage", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OnJoinMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IChatRoomService", "OnJoin", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OnLeaveMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IChatRoomService", "OnLeave", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            OnMessageReceivedMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IChatRoomService", "OnMessageReceived", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        IChatRoomServiceClient()
        {
        }

        public IChatRoomServiceClient(CallInvoker callInvoker, IFormatterResolver resolver)
            : base(callInvoker, resolver)
        {
        }

        protected override MagicOnionClientBase<IChatRoomService> Clone()
        {
            var clone = new IChatRoomServiceClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            return clone;
        }
   
        public UnaryResult<global::Sandbox.ChatRoomResponse> CreateNewRoom(string roomName, string nickName)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<string, string>(roomName, nickName), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(CreateNewRoomMethod, base.host, base.option, __request);
            return new UnaryResult<global::Sandbox.ChatRoomResponse>(__callResult, base.resolver);
        }

        public UnaryResult<global::Sandbox.ChatRoomResponse> Join(string roomId, string nickName)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<string, string>(roomId, nickName), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(JoinMethod, base.host, base.option, __request);
            return new UnaryResult<global::Sandbox.ChatRoomResponse>(__callResult, base.resolver);
        }

        public UnaryResult<global::Sandbox.ChatRoomResponse[]> GetRooms()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncUnaryCall(GetRoomsMethod, base.host, base.option, __request);
            return new UnaryResult<global::Sandbox.ChatRoomResponse[]>(__callResult, base.resolver);
        }

        public UnaryResult<bool> Leave(string roomId)
        {
            var __request = MessagePackSerializer.Serialize(roomId, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(LeaveMethod, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, base.resolver);
        }

        public UnaryResult<bool> SendMessage(string roomId, string message)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<string, string>(roomId, message), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(SendMessageMethod, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::Sandbox.RoomMember> OnJoin()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncServerStreamingCall(OnJoinMethod, base.host, base.option, __request);
            return new ServerStreamingResult<global::Sandbox.RoomMember>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::Sandbox.RoomMember> OnLeave()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncServerStreamingCall(OnLeaveMethod, base.host, base.option, __request);
            return new ServerStreamingResult<global::Sandbox.RoomMember>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::Sandbox.ChatMessage> OnMessageReceived()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncServerStreamingCall(OnMessageReceivedMethod, base.host, base.option, __request);
            return new ServerStreamingResult<global::Sandbox.ChatMessage>(__callResult, base.resolver);
        }

    }


    public class IHeartbeatClient : MagicOnionClientBase<IHeartbeat>, IHeartbeat
    {
        static readonly Method<byte[], byte[]> ConnectMethod;
        static readonly Method<byte[], byte[]> TestSendMethod;

        static IHeartbeatClient()
        {
            ConnectMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IHeartbeat", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            TestSendMethod = new Method<byte[], byte[]>(MethodType.Unary, "IHeartbeat", "TestSend", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        IHeartbeatClient()
        {
        }

        public IHeartbeatClient(CallInvoker callInvoker, IFormatterResolver resolver)
            : base(callInvoker, resolver)
        {
        }

        protected override MagicOnionClientBase<IHeartbeat> Clone()
        {
            var clone = new IHeartbeatClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            return clone;
        }
   
        public DuplexStreamingResult<global::MessagePack.Nil, global::MessagePack.Nil> Connect()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(ConnectMethod, base.host, base.option);
            return new DuplexStreamingResult<global::MessagePack.Nil, global::MessagePack.Nil>(__callResult, base.resolver);
        }

        public UnaryResult<global::MessagePack.Nil> TestSend(string connectionId)
        {
            var __request = MessagePackSerializer.Serialize(connectionId, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(TestSendMethod, base.host, base.option, __request);
            return new UnaryResult<global::MessagePack.Nil>(__callResult, base.resolver);
        }

    }


    public class ISendMetadataClient : MagicOnionClientBase<ISendMetadata>, ISendMetadata
    {
        static readonly Method<byte[], byte[]> PangPongMethod;

        static ISendMetadataClient()
        {
            PangPongMethod = new Method<byte[], byte[]>(MethodType.Unary, "ISendMetadata", "PangPong", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        ISendMetadataClient()
        {
        }

        public ISendMetadataClient(CallInvoker callInvoker, IFormatterResolver resolver)
            : base(callInvoker, resolver)
        {
        }

        protected override MagicOnionClientBase<ISendMetadata> Clone()
        {
            var clone = new ISendMetadataClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            return clone;
        }
   
        public UnaryResult<int> PangPong()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncUnaryCall(PangPongMethod, base.host, base.option, __request);
            return new UnaryResult<int>(__callResult, base.resolver);
        }

    }


    public class IStandardClient : MagicOnionClientBase<IStandard>, IStandard
    {
        static readonly Method<byte[], byte[]> Unary1AsyncMethod;
        static readonly Method<byte[], byte[]> ClientStreaming1AsyncMethod;
        static readonly Method<byte[], byte[]> ServerStreamingAsyncMethod;
        static readonly Method<byte[], byte[]> DuplexStreamingAsyncMethod;

        static IStandardClient()
        {
            Unary1AsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IStandard", "Unary1Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ClientStreaming1AsyncMethod = new Method<byte[], byte[]>(MethodType.ClientStreaming, "IStandard", "ClientStreaming1Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ServerStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IStandard", "ServerStreamingAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            DuplexStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IStandard", "DuplexStreamingAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        IStandardClient()
        {
        }

        public IStandardClient(CallInvoker callInvoker, IFormatterResolver resolver)
            : base(callInvoker, resolver)
        {
        }

        protected override MagicOnionClientBase<IStandard> Clone()
        {
            var clone = new IStandardClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            return clone;
        }
   
        public UnaryResult<int> Unary1Async(int x, int y)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int>(x, y), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(Unary1AsyncMethod, base.host, base.option, __request);
            return new UnaryResult<int>(__callResult, base.resolver);
        }

        public ClientStreamingResult<int, string> ClientStreaming1Async()
        {
            var __callResult = callInvoker.AsyncClientStreamingCall<byte[], byte[]>(ClientStreaming1AsyncMethod, base.host, base.option);
            return new ClientStreamingResult<int, string>(__callResult, base.resolver);
        }

        public ServerStreamingResult<int> ServerStreamingAsync(int x, int y, int z)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, int>(x, y, z), base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingAsyncMethod, base.host, base.option, __request);
            return new ServerStreamingResult<int>(__callResult, base.resolver);
        }

        public DuplexStreamingResult<int, int> DuplexStreamingAsync()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, base.host, base.option);
            return new DuplexStreamingResult<int, int>(__callResult, base.resolver);
        }

    }


    public class IMyFirstServiceClient : MagicOnionClientBase<IMyFirstService>, IMyFirstService
    {
        static readonly Method<byte[], byte[]> SumAsyncMethod;
        static readonly Method<byte[], byte[]> SumAsync2Method;
        static readonly Method<byte[], byte[]> StreamingOneMethod;
        static readonly Method<byte[], byte[]> StreamingTwoMethod;
        static readonly Method<byte[], byte[]> StreamingTwo2Method;
        static readonly Method<byte[], byte[]> StreamingThreeMethod;

        static IMyFirstServiceClient()
        {
            SumAsyncMethod = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "SumAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            SumAsync2Method = new Method<byte[], byte[]>(MethodType.Unary, "IMyFirstService", "SumAsync2", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            StreamingOneMethod = new Method<byte[], byte[]>(MethodType.ClientStreaming, "IMyFirstService", "StreamingOne", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            StreamingTwoMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IMyFirstService", "StreamingTwo", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            StreamingTwo2Method = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IMyFirstService", "StreamingTwo2", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            StreamingThreeMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IMyFirstService", "StreamingThree", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        IMyFirstServiceClient()
        {
        }

        public IMyFirstServiceClient(CallInvoker callInvoker, IFormatterResolver resolver)
            : base(callInvoker, resolver)
        {
        }

        protected override MagicOnionClientBase<IMyFirstService> Clone()
        {
            var clone = new IMyFirstServiceClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            return clone;
        }
   
        public UnaryResult<string> SumAsync(int x, int y)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int>(x, y), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(SumAsyncMethod, base.host, base.option, __request);
            return new UnaryResult<string>(__callResult, base.resolver);
        }

        public UnaryResult<string> SumAsync2(int x, int y)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int>(x, y), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(SumAsync2Method, base.host, base.option, __request);
            return new UnaryResult<string>(__callResult, base.resolver);
        }

        public ClientStreamingResult<int, string> StreamingOne()
        {
            var __callResult = callInvoker.AsyncClientStreamingCall<byte[], byte[]>(StreamingOneMethod, base.host, base.option);
            return new ClientStreamingResult<int, string>(__callResult, base.resolver);
        }

        public ServerStreamingResult<string> StreamingTwo(int x, int y, int z)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, int>(x, y, z), base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(StreamingTwoMethod, base.host, base.option, __request);
            return new ServerStreamingResult<string>(__callResult, base.resolver);
        }

        public ServerStreamingResult<string> StreamingTwo2(int x, int y, int z = 9999)
        {
            var __request = MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, int>(x, y, z), base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(StreamingTwo2Method, base.host, base.option, __request);
            return new ServerStreamingResult<string>(__callResult, base.resolver);
        }

        public DuplexStreamingResult<int, string> StreamingThree()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(StreamingThreeMethod, base.host, base.option);
            return new DuplexStreamingResult<int, string>(__callResult, base.resolver);
        }

    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
