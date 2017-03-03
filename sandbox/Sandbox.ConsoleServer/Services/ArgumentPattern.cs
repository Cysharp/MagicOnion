using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;
using SharedLibrary;
using MessagePack;

namespace Sandbox.ConsoleServer.Services
{
    public class ArgumentPattern : ServiceBase<IArgumentPattern>, IArgumentPattern
    {
        public async Task<ServerStreamingResult<MyResponse>> ServerStreamingResult1(int x, int y, string z = "unknown")
        {
            var stream = GetServerStreamingContext<MyResponse>();
            await stream.WriteAsync(new MyResponse { Id = x + y, Data = z });
            return stream.Result();
        }

        public async Task<ServerStreamingResult<MyResponse>> ServerStreamingResult2(MyRequest req)
        {
            var stream = GetServerStreamingContext<MyResponse>();
            await stream.WriteAsync(new MyResponse { Id = req.Id, Data = req.Data });
            return stream.Result();
        }

        public async Task<ServerStreamingResult<MyResponse>> ServerStreamingResult3()
        {
            var stream = GetServerStreamingContext<MyResponse>();
            await stream.WriteAsync(new MyResponse { Id = -1, Data = "empty" });
            return stream.Result();
        }

        public async Task<ServerStreamingResult<Nil>> ServerStreamingResult4()
        {
            var stream = GetServerStreamingContext<Nil>();
            await stream.WriteAsync(Nil.Default);
            return stream.Result();
        }

        public async Task<ServerStreamingResult<MyStructResponse>> ServerStreamingResult5(MyStructRequest req)
        {
            var stream = GetServerStreamingContext<MyStructResponse>();
            await stream.WriteAsync(new MyStructResponse { X = req.X, Y = req.Y });
            return stream.Result();
        }

        public UnaryResult<MyHugeResponse> Unary1(int x, int y, string z = "unknown", MyEnum e = MyEnum.Orange, MyStructResponse soho = default(MyStructResponse), ulong zzz = 9, MyRequest req = null)
        {
            return UnaryResult(new MyHugeResponse
            {
                x = x,
                y = y,
                z = z,
                e = e,
                soho = soho,
                zzz = zzz,
                req = req
            });
        }

        public UnaryResult<MyResponse> Unary2(MyRequest req)
        {
            return UnaryResult(new MyResponse
            {
                Id = req.Id,
                Data = req.Data
            });
        }

        public UnaryResult<MyResponse> Unary3()
        {
            return UnaryResult(new MyResponse
            {
                Id = -1,
                Data = "empty"
            });
        }

        public UnaryResult<MyStructResponse> Unary5(MyStructRequest req)
        {
            return UnaryResult(new MyStructResponse
            {
                X = req.X,
                Y = req.Y
            });
        }

        public UnaryResult<bool> UnaryS1(DateTime dt, DateTimeOffset dt2)
        {
            return UnaryResult(true);
        }

        public UnaryResult<bool> UnaryS2(int[] arrayPattern)
        {
            return UnaryResult(true);
        }

        public UnaryResult<bool> UnaryS3(int[] arrayPattern1, string[] arrayPattern2, MyEnum[] arrayPattern3)
        {
            return UnaryResult(true);
        }
    }
}
