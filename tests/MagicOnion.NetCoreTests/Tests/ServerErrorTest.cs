using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace MagicOnion.Tests
{
    public interface IOverloadMethod : IService<IOverloadMethod>
    {
        UnaryResult<int> Hoge(int x);
        UnaryResult<int> Hoge(int x, int y);
    }

    [Ignore]
    public class OverloadService : ServiceBase<IOverloadMethod>, IOverloadMethod
    {
        public UnaryResult<int> Hoge(int x)
        {
            return UnaryResult(0);
        }

        public UnaryResult<int> Hoge(int x, int y)
        {
            return UnaryResult(0);
        }
    }

    public interface IArgumentMethod : IService<IArgumentMethod>
    {
        UnaryResult<int> Hoge(int x);
        ServerStreamingResult<int> Huga(int x);
        ClientStreamingResult<int, int> Tako(int x);
        DuplexStreamingResult<int, int> Nano(int x);
    }

    public class ArgumentMethodService : ServiceBase<IArgumentMethod>, IArgumentMethod
    {
        public UnaryResult<int> Hoge(int x)
        {
            throw new NotImplementedException();
        }

        public ServerStreamingResult<int> Huga(int x)
        {
            throw new NotImplementedException();
        }

        [Ignore]
        public DuplexStreamingResult<int, int> Nano(int x)
        {
            throw new NotImplementedException();
        }

        [Ignore]
        public ClientStreamingResult<int, int> Tako(int x)
        {
            throw new NotImplementedException();
        }
    }

    public class ServerErrorTest
    {
        //[Fact]
        //public async Task Moge()
        //{
        //    var service = MagicOnionEngine.BuildServerServiceDefinition(true);

        //    var server = new global::Grpc.Core.Server
        //    {
        //        Services = { service },
        //        Ports = { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }
        //    };

        //    server.Start();

        //    //var channel = new Channel("localhost:12345", ChannelCredentials.Insecure);
        //    //await MagicOnionClient.Create<IOverloadMethod>(channel).Hoge(0);
        //}
    }
}
