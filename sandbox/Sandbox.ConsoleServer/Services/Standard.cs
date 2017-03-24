#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;

namespace Sandbox.ConsoleServer.Services
{
    public class Standard : ServiceBase<IStandard>, IStandard
    {
        public async Task<ClientStreamingResult<int, string>> ClientStreaming1Async()
        {
            var stream = GetClientStreamingContext<int, string>();
            var l = new List<int>();
            await stream.ForEachAsync(x =>
            {
                l.Add(x);
            });

            return stream.Result(string.Join(", ", l));
        }

        public async Task<DuplexStreamingResult<int, int>> DuplexStreamingAsync()
        {
            var stream = GetDuplexStreamingContext<int, int>();

            await stream.ForEachAsync(async x =>
            {
                await stream.WriteAsync(x);
            });

            return stream.Result();
        }

        public async Task<ServerStreamingResult<int>> ServerStreamingAsync(int x, int y, int z)
        {
            var stream = GetServerStreamingContext<int>();
            for (int i = 0; i < z; i++)
            {
                await stream.WriteAsync((x + y));
            }

            return stream.Result();
        }













        // server:
        // return int but return type is UnaryResult<int>
        // no more Task<UnaryResult<int>>, everything naturally

        public async UnaryResult<int> Unary1(int x, int y)
        {
            return x + y;
        }

        public async UnaryResult<int> Unary2(int x, int y)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            return x + y;
        }
















    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously