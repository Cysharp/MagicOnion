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
            MagicOnionClientRegistry<Sandbox.ConsoleServer.ITetDefinition>.Register((x, y) => new Sandbox.ConsoleServer.ITetDefinitionClient(x, y));
        }
    }
}
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MagicOnion.Resolvers
{
    using System;
    using MessagePack;

    public class MagicOnionResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new MagicOnionResolver();

        MagicOnionResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = MagicOnionResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class MagicOnionResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static MagicOnionResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(16)
            {
                {typeof(global::MagicOnion.DynamicArgumentTuple<global::System.Collections.Generic.List<int>, global::System.Collections.Generic.Dictionary<int, int>>), 0 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<global::System.DateTime, global::System.DateTimeOffset>), 1 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, int, int>), 2 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, int, string, global::SharedLibrary.MyEnum, global::SharedLibrary.MyStructResponse, ulong, global::SharedLibrary.MyRequest>), 3 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, int, string>), 4 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, int>), 5 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int[], string[], global::SharedLibrary.MyEnum[]>), 6 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<string, int, int, global::SharedLibrary.MyEnum2>), 7 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<string, string>), 8 },
                {typeof(global::Sandbox.ChatRoomResponse[]), 9 },
                {typeof(global::SharedLibrary.MyEnum?), 10 },
                {typeof(global::SharedLibrary.MyEnum[]), 11 },
                {typeof(global::System.Collections.Generic.Dictionary<int, int>), 12 },
                {typeof(global::System.Collections.Generic.List<int>), 13 },
                {typeof(global::SharedLibrary.MyEnum), 14 },
                {typeof(global::SharedLibrary.MyEnum2), 15 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                if (t == typeof(UniRx.Unit))
                {
                    return MagicOnion.Resolvers.UniRxIntegrate.UnitFormatter.Instance;
                }
                else if (t == typeof(Nullable<UniRx.Unit>))
                {
                    return MagicOnion.Resolvers.UniRxIntegrate.NullableUnitFormatter.Instance;
                }
                return null;
            }

            switch (key)
            {
                case 0: return new global::MagicOnion.DynamicArgumentTupleFormatter<global::System.Collections.Generic.List<int>, global::System.Collections.Generic.Dictionary<int, int>>(default(global::System.Collections.Generic.List<int>), default(global::System.Collections.Generic.Dictionary<int, int>));
                case 1: return new global::MagicOnion.DynamicArgumentTupleFormatter<global::System.DateTime, global::System.DateTimeOffset>(default(global::System.DateTime), default(global::System.DateTimeOffset));
                case 2: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, int, int>(default(int), default(int), default(int));
                case 3: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, int, string, global::SharedLibrary.MyEnum, global::SharedLibrary.MyStructResponse, ulong, global::SharedLibrary.MyRequest>(default(int), default(int), default(string), default(global::SharedLibrary.MyEnum), default(global::SharedLibrary.MyStructResponse), default(ulong), default(global::SharedLibrary.MyRequest));
                case 4: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, int, string>(default(int), default(int), default(string));
                case 5: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, int>(default(int), default(int));
                case 6: return new global::MagicOnion.DynamicArgumentTupleFormatter<int[], string[], global::SharedLibrary.MyEnum[]>(default(int[]), default(string[]), default(global::SharedLibrary.MyEnum[]));
                case 7: return new global::MagicOnion.DynamicArgumentTupleFormatter<string, int, int, global::SharedLibrary.MyEnum2>(default(string), default(int), default(int), default(global::SharedLibrary.MyEnum2));
                case 8: return new global::MagicOnion.DynamicArgumentTupleFormatter<string, string>(default(string), default(string));
                case 9: return new global::MessagePack.Formatters.ArrayFormatter<global::Sandbox.ChatRoomResponse>();
                case 10: return new global::MessagePack.Formatters.NullableFormatter<global::SharedLibrary.MyEnum>();
                case 11: return new global::MessagePack.Formatters.ArrayFormatter<global::SharedLibrary.MyEnum>();
                case 12: return new global::MessagePack.Formatters.DictionaryFormatter<int, int>();
                case 13: return new global::MessagePack.Formatters.ListFormatter<int>();
                case 14: return new MagicOnion.Formatters.SharedLibrary.MyEnumFormatter();
                case 15: return new MagicOnion.Formatters.SharedLibrary.MyEnum2Formatter();
                default: return null;
            }
        }
    }
}

namespace MagicOnion.Resolvers.UniRxIntegrate
{
    using System;
    using UniRx;
    using MessagePack;
    using MessagePack.Formatters;

    public class UnitFormatter : IMessagePackFormatter<Unit>
    {
        public static readonly IMessagePackFormatter<Unit> Instance = new UnitFormatter();

        UnitFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, Unit value, IFormatterResolver typeResolver)
        {
            return MessagePackBinary.WriteNil(ref bytes, offset);
        }

        public Unit Deserialize(byte[] bytes, int offset, IFormatterResolver typeResolver, out int readSize)
        {
            if (bytes[offset] == MessagePackCode.Nil)
            {
                readSize = 1;
                return Unit.Default;
            }
            else
            {
                throw new InvalidOperationException(string.Format("code is invalid. code:{0} format:{1}", bytes[offset], MessagePackCode.ToFormatName(bytes[offset])));
            }
        }
    }

    public class NullableUnitFormatter : IMessagePackFormatter<Unit?>
    {
        public static readonly IMessagePackFormatter<Unit?> Instance = new NullableUnitFormatter();

        NullableUnitFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, Unit? value, IFormatterResolver typeResolver)
        {
            return MessagePackBinary.WriteNil(ref bytes, offset);
        }

        public Unit? Deserialize(byte[] bytes, int offset, IFormatterResolver typeResolver, out int readSize)
        {
            if (bytes[offset] == MessagePackCode.Nil)
            {
                readSize = 1;
                return Unit.Default;
            }
            else
            {
                throw new InvalidOperationException(string.Format("code is invalid. code:{0} format:{1}", bytes[offset], MessagePackCode.ToFormatName(bytes[offset])));
            }
        }
    }
}


#pragma warning disable 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MagicOnion.Formatters.SharedLibrary
{
    using System;
    using MessagePack;

    public sealed class MyEnumFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::SharedLibrary.MyEnum>
    {
        public int Serialize(ref byte[] bytes, int offset, global::SharedLibrary.MyEnum value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::SharedLibrary.MyEnum Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::SharedLibrary.MyEnum)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class MyEnum2Formatter : global::MessagePack.Formatters.IMessagePackFormatter<global::SharedLibrary.MyEnum2>
    {
        public int Serialize(ref byte[] bytes, int offset, global::SharedLibrary.MyEnum2 value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::SharedLibrary.MyEnum2 Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::SharedLibrary.MyEnum2)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }


}

#pragma warning disable 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
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
   
        ServerStreamingResult<global::UniRx.Unit> ServerStreamingResult4();
   
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
   
        DuplexStreamingResult<global::MessagePack.Nil, global::UniRx.Unit> Connect();
   
        UnaryResult<global::UniRx.Unit> TestSend(string connectionId);
    }




    public interface ISendMetadata : MagicOnion.IService<Sandbox.ConsoleServer.ISendMetadata>
    {
   
        UnaryResult<int> PangPong();
    }




    public interface IStandard : MagicOnion.IService<Sandbox.ConsoleServer.IStandard>
    {
   
        UnaryResult<int> Unary1(int x, int y);
   
        UnaryResult<int> Unary2(int x, int y);
   
        ClientStreamingResult<int, string> ClientStreaming1Async();
   
        ServerStreamingResult<int> ServerStreamingAsync(int x, int y, int z);
   
        DuplexStreamingResult<int, int> DuplexStreamingAsync();
   
        UnaryResult<global::SharedLibrary.MyClass2> Echo(string name, int x, int y, global::SharedLibrary.MyEnum2 e);
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




    public interface ITetDefinition : MagicOnion.IService<Sandbox.ConsoleServer.ITetDefinition>
    {
   
        UnaryResult<global::SharedLibrary.MyEnum?> Test(global::System.Collections.Generic.List<int> l, global::System.Collections.Generic.Dictionary<int, int> d);
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
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, string, global::SharedLibrary.MyEnum, global::SharedLibrary.MyStructResponse, ulong, global::SharedLibrary.MyRequest>(x, y, z, e, soho, zzz, req), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(Unary1Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyHugeResponse>(__callResult, base.resolver);
        }

        public UnaryResult<global::SharedLibrary.MyResponse> Unary2(global::SharedLibrary.MyRequest req)
        {
            var __request = LZ4MessagePackSerializer.Serialize(req, base.resolver);
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
            var __request = LZ4MessagePackSerializer.Serialize(req, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(Unary5Method, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyStructResponse>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult1(int x, int y, string z = "unknown")
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, string>(x, y, z), base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult1Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyResponse>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult2(global::SharedLibrary.MyRequest req)
        {
            var __request = LZ4MessagePackSerializer.Serialize(req, base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult2Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyResponse>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::SharedLibrary.MyResponse> ServerStreamingResult3()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult3Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyResponse>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::UniRx.Unit> ServerStreamingResult4()
        {
            var __request = MagicOnionMarshallers.UnsafeNilBytes;
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult4Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::UniRx.Unit>(__callResult, base.resolver);
        }

        public ServerStreamingResult<global::SharedLibrary.MyStructResponse> ServerStreamingResult5(global::SharedLibrary.MyStructRequest req)
        {
            var __request = LZ4MessagePackSerializer.Serialize(req, base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingResult5Method, base.host, base.option, __request);
            return new ServerStreamingResult<global::SharedLibrary.MyStructResponse>(__callResult, base.resolver);
        }

        public UnaryResult<bool> UnaryS1(global::System.DateTime dt, global::System.DateTimeOffset dt2)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<global::System.DateTime, global::System.DateTimeOffset>(dt, dt2), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(UnaryS1Method, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, base.resolver);
        }

        public UnaryResult<bool> UnaryS2(int[] arrayPattern)
        {
            var __request = LZ4MessagePackSerializer.Serialize(arrayPattern, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(UnaryS2Method, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, base.resolver);
        }

        public UnaryResult<bool> UnaryS3(int[] arrayPattern1, string[] arrayPattern2, global::SharedLibrary.MyEnum[] arrayPattern3)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int[], string[], global::SharedLibrary.MyEnum[]>(arrayPattern1, arrayPattern2, arrayPattern3), base.resolver);
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
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<string, string>(roomName, nickName), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(CreateNewRoomMethod, base.host, base.option, __request);
            return new UnaryResult<global::Sandbox.ChatRoomResponse>(__callResult, base.resolver);
        }

        public UnaryResult<global::Sandbox.ChatRoomResponse> Join(string roomId, string nickName)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<string, string>(roomId, nickName), base.resolver);
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
            var __request = LZ4MessagePackSerializer.Serialize(roomId, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(LeaveMethod, base.host, base.option, __request);
            return new UnaryResult<bool>(__callResult, base.resolver);
        }

        public UnaryResult<bool> SendMessage(string roomId, string message)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<string, string>(roomId, message), base.resolver);
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
   
        public DuplexStreamingResult<global::MessagePack.Nil, global::UniRx.Unit> Connect()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(ConnectMethod, base.host, base.option);
            return new DuplexStreamingResult<global::MessagePack.Nil, global::UniRx.Unit>(__callResult, base.resolver);
        }

        public UnaryResult<global::UniRx.Unit> TestSend(string connectionId)
        {
            var __request = LZ4MessagePackSerializer.Serialize(connectionId, base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(TestSendMethod, base.host, base.option, __request);
            return new UnaryResult<global::UniRx.Unit>(__callResult, base.resolver);
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
        static readonly Method<byte[], byte[]> Unary1Method;
        static readonly Method<byte[], byte[]> Unary2Method;
        static readonly Method<byte[], byte[]> ClientStreaming1AsyncMethod;
        static readonly Method<byte[], byte[]> ServerStreamingAsyncMethod;
        static readonly Method<byte[], byte[]> DuplexStreamingAsyncMethod;
        static readonly Method<byte[], byte[]> EchoMethod;

        static IStandardClient()
        {
            Unary1Method = new Method<byte[], byte[]>(MethodType.Unary, "IStandard", "Unary1", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            Unary2Method = new Method<byte[], byte[]>(MethodType.Unary, "IStandard", "Unary2", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ClientStreaming1AsyncMethod = new Method<byte[], byte[]>(MethodType.ClientStreaming, "IStandard", "ClientStreaming1Async", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ServerStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.ServerStreaming, "IStandard", "ServerStreamingAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            DuplexStreamingAsyncMethod = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IStandard", "DuplexStreamingAsync", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            EchoMethod = new Method<byte[], byte[]>(MethodType.Unary, "IStandard", "Echo", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
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
   
        public UnaryResult<int> Unary1(int x, int y)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int>(x, y), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(Unary1Method, base.host, base.option, __request);
            return new UnaryResult<int>(__callResult, base.resolver);
        }

        public UnaryResult<int> Unary2(int x, int y)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int>(x, y), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(Unary2Method, base.host, base.option, __request);
            return new UnaryResult<int>(__callResult, base.resolver);
        }

        public ClientStreamingResult<int, string> ClientStreaming1Async()
        {
            var __callResult = callInvoker.AsyncClientStreamingCall<byte[], byte[]>(ClientStreaming1AsyncMethod, base.host, base.option);
            return new ClientStreamingResult<int, string>(__callResult, base.resolver);
        }

        public ServerStreamingResult<int> ServerStreamingAsync(int x, int y, int z)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, int>(x, y, z), base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(ServerStreamingAsyncMethod, base.host, base.option, __request);
            return new ServerStreamingResult<int>(__callResult, base.resolver);
        }

        public DuplexStreamingResult<int, int> DuplexStreamingAsync()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(DuplexStreamingAsyncMethod, base.host, base.option);
            return new DuplexStreamingResult<int, int>(__callResult, base.resolver);
        }

        public UnaryResult<global::SharedLibrary.MyClass2> Echo(string name, int x, int y, global::SharedLibrary.MyEnum2 e)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<string, int, int, global::SharedLibrary.MyEnum2>(name, x, y, e), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(EchoMethod, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyClass2>(__callResult, base.resolver);
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
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int>(x, y), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(SumAsyncMethod, base.host, base.option, __request);
            return new UnaryResult<string>(__callResult, base.resolver);
        }

        public UnaryResult<string> SumAsync2(int x, int y)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int>(x, y), base.resolver);
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
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, int>(x, y, z), base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(StreamingTwoMethod, base.host, base.option, __request);
            return new ServerStreamingResult<string>(__callResult, base.resolver);
        }

        public ServerStreamingResult<string> StreamingTwo2(int x, int y, int z = 9999)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, int, int>(x, y, z), base.resolver);
            var __callResult = callInvoker.AsyncServerStreamingCall(StreamingTwo2Method, base.host, base.option, __request);
            return new ServerStreamingResult<string>(__callResult, base.resolver);
        }

        public DuplexStreamingResult<int, string> StreamingThree()
        {
            var __callResult = callInvoker.AsyncDuplexStreamingCall<byte[], byte[]>(StreamingThreeMethod, base.host, base.option);
            return new DuplexStreamingResult<int, string>(__callResult, base.resolver);
        }

    }


    public class ITetDefinitionClient : MagicOnionClientBase<ITetDefinition>, ITetDefinition
    {
        static readonly Method<byte[], byte[]> TestMethod;

        static ITetDefinitionClient()
        {
            TestMethod = new Method<byte[], byte[]>(MethodType.Unary, "ITetDefinition", "Test", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
        }

        ITetDefinitionClient()
        {
        }

        public ITetDefinitionClient(CallInvoker callInvoker, IFormatterResolver resolver)
            : base(callInvoker, resolver)
        {
        }

        protected override MagicOnionClientBase<ITetDefinition> Clone()
        {
            var clone = new ITetDefinitionClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.resolver = this.resolver;
            return clone;
        }
   
        public UnaryResult<global::SharedLibrary.MyEnum?> Test(global::System.Collections.Generic.List<int> l, global::System.Collections.Generic.Dictionary<int, int> d)
        {
            var __request = LZ4MessagePackSerializer.Serialize(new DynamicArgumentTuple<global::System.Collections.Generic.List<int>, global::System.Collections.Generic.Dictionary<int, int>>(l, d), base.resolver);
            var __callResult = callInvoker.AsyncUnaryCall(TestMethod, base.host, base.option, __request);
            return new UnaryResult<global::SharedLibrary.MyEnum?>(__callResult, base.resolver);
        }

    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
