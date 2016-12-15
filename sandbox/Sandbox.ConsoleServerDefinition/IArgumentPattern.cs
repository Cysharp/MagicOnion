using MagicOnion;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
    public interface IArgumentPattern : IService<IArgumentPattern>
    {
        UnaryResult<MyHugeResponse> Unary1(int x, int y, string z = "unknown", MyEnum e = MyEnum.Orange, MyStructResponse soho = default(MyStructResponse), ulong zzz = 9, MyRequest req = null);
        UnaryResult<MyResponse> Unary2(MyRequest req);
        UnaryResult<MyResponse> Unary3();
        UnaryResult<MyStructResponse> Unary5(MyStructRequest req);
        Task<ServerStreamingResult<MyResponse>> ServerStreamingResult1(int x, int y, string z = "unknown");
        Task<ServerStreamingResult<MyResponse>> ServerStreamingResult2(MyRequest req);
        Task<ServerStreamingResult<MyResponse>> ServerStreamingResult3();
        Task<ServerStreamingResult<Nil>> ServerStreamingResult4();
        Task<ServerStreamingResult<MyStructResponse>> ServerStreamingResult5(MyStructRequest req);
    }
}