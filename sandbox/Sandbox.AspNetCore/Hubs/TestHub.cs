using MagicOnion;
using MagicOnion.Server.Hubs;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.AspNetCore.Hubs
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
}
