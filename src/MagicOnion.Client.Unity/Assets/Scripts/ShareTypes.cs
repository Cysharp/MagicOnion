using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace SharedLibrary
{
    [MessagePackObject]
    public class MyRequest
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Data { get; set; }
    }

    [MessagePackObject]
    public struct MyStructRequest
    {
        [Key(0)]
        public int X;
        [Key(1)]
        public int Y;

        public MyStructRequest(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [MessagePackObject]
    public struct MyStructResponse
    {
        [Key(0)]
        public int X;
        [Key(1)]
        public int Y;

        public MyStructResponse(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [MessagePackObject]
    public class MyResponse
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Data { get; set; }
    }

    [MessagePackObject]
    public class MyHugeResponse
    {
        [Key(0)]
        public int x { get; set; }
        [Key(1)]
        public int y { get; set; }
        [Key(2)]
        public string z { get; set; }
        [Key(3)]
        public MyEnum e { get; set; }
        [Key(4)]
        public MyStructResponse soho { get; set; }
        [Key(5)]
        public ulong zzz { get; set; }
        [Key(6)]
        public MyRequest req { get; set; }
    }

    public enum MyEnum
    {
        Apple, Orange, Grape
    }

    [MessagePackObject]
    public class MyClass2
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public int Sum { get; set; }
    }

    public enum MyEnum2
    {
        A = 2,
        B = 3,
        C = 4
    }
}

namespace Sandbox.NetCoreServer.Hubs
{
    public interface IMessageReceiver
    {
        Task ZeroArgument();
        Task OneArgument(int x);
        Task MoreArgument(int x, string y, double z);
        void VoidZeroArgument();
        void VoidOneArgument(int x);
        void VoidMoreArgument(int x, string y, double z);
        Task OneArgument2(TestObject x);
        void VoidOneArgument2(TestObject x);
        Task OneArgument3(TestObject[] x);
        void VoidOneArgument3(TestObject[] x);
    }

    public interface ITestHub : IStreamingHub<ITestHub, IMessageReceiver>
    {
        Task ZeroArgument();
        Task OneArgument(int x);
        Task MoreArgument(int x, string y, double z);

        Task<int> RetrunZeroArgument();
        Task<string> RetrunOneArgument(int x);
        Task<double> RetrunMoreArgument(int x, string y, double z);

        Task OneArgument2(TestObject x);
        Task<TestObject> RetrunOneArgument2(TestObject x);

        Task OneArgument3(TestObject[] x);
        Task<TestObject[]> RetrunOneArgument3(TestObject[] x);
    }

    [MessagePackObject]
    public class TestObject
    {
        [Key(0)]
        public int X { get; set; }
        [Key(1)]
        public int Y { get; set; }
        [Key(2)]
        public int Z { get; set; }
    }

    public enum TestEnum
    {

    }


    public class OreOreRequest
    {

    }


    public class OreOreResponse
    {

    }
}

namespace Sandbox.NetCoreServer.Services
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<Nil> ZeroAsync();
        UnaryResult<TestEnum> OneAsync(int z);
        UnaryResult<string> SumAsync(int x, int y);
        UnaryResult<OreOreResponse> OreOreAsync(OreOreRequest z);
        UnaryResult<OreOreResponse[]> OreOre2Async(OreOreRequest z);
        UnaryResult<List<OreOreResponse>> OreOre3Async(OreOreRequest z);


        Task<UnaryResult<Nil>> LegacyZeroAsync();
        Task<UnaryResult<TestEnum>> LegacyOneAsync(int z);
        Task<UnaryResult<string>> LegacySumAsync(int x, int y);
        Task<UnaryResult<OreOreResponse>> LegacyOreOreAsync(OreOreRequest z);
        Task<UnaryResult<OreOreResponse[]>> LegacyOreOre2Async(OreOreRequest z);
        Task<UnaryResult<List<OreOreResponse>>> LegacyOreOre3Async(OreOreRequest z);

        // use hub instead:)

        Task<ClientStreamingResult<int, string>> ClientStreamingSampleAsync();
        Task<ServerStreamingResult<string>> ServertSreamingSampleAsync(int x, int y, int z);
        Task<DuplexStreamingResult<int, string>> DuplexStreamingSampleAync();
    }

    public enum TestEnum
    {

    }


    public class OreOreRequest
    {

    }


    public class OreOreResponse
    {

    }
}

namespace Sandbox.NetCoreServer.Hubs
{
    public interface IMessageReceiver2
    {
        void OnReceiveMessage(string senderUser, string message);
    }

    public interface IChatHub : IStreamingHub<IChatHub, IMessageReceiver2>
    {
        Task JoinAsync(string userName, string roomName);
        Task LeaveAsync();
        Task SendMessageAsync(string message);
    }
}
