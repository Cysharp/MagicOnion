using Grpc.Core;
using FluentAssertions;
using MagicOnion.Client;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Tests
{
    [MessagePackObject]
    public class SerializableCallContext
    {
        [Key(0)]
        public virtual DateTime Deadline { get; set; }
        [Key(1)]
        public virtual string Host { get; set; }
        [Key(2)]
        public virtual string Method { get; set; }
        [Key(3)]
        public virtual string Peer { get; set; }
        [Key(4)]
        public virtual Dictionary<string, string> RequestHeaders { get; set; }
        [Key(5)]
        public virtual Dictionary<string, string> ResponseTrailers { get; set; }
    }

    public interface IConfigurationChange : IService<IConfigurationChange>
    {
        UnaryResult<SerializableCallContext> ReturnContext();
    }

    public class ConfigurationChange : ServiceBase<IConfigurationChange>, IConfigurationChange
    {
        public UnaryResult<SerializableCallContext> ReturnContext()
        {
            var context = this.Context.CallContext;
            var result = new SerializableCallContext
            {
                Deadline = context.Deadline,
                Host = context.Host,
                Method = context.Method,
                Peer = context.Peer,
                RequestHeaders = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value),
                ResponseTrailers = context.ResponseTrailers.ToDictionary(x => x.Key, x => x.Value),
            };

            return UnaryResult(result);
        }
    }

    [Collection(nameof(AllAssemblyGrpcServerFixture))]
    public class ClientConfigTest
    {
        ITestOutputHelper logger;
        IConfigurationChange client;

        public ClientConfigTest(ITestOutputHelper logger, ServerFixture server)
        {
            this.logger = logger;
            this.client = server.CreateClient<IConfigurationChange>();
        }

        /*
        protected string host;
        protected CallOptions option;
        protected CallInvoker callInvoker;
        */

        //[Fact]
        //public async Task WithHost()
        //{
        //    (client.Should().Be( .AsDynamic().host as string).IsNull();

        //    var hostChange = client.WithHost("newhost");
        //    (hostChange.AsDynamic().host as string).Should().Be("newhost");

        //    var serverContext = await hostChange.ReturnContext();
        //    serverContext.Host.Should().Be("newhost");
        //}

        //[Fact]
        //public void WithCancellation()
        //{
        //    var cts = new CancellationTokenSource();
        //    var tk = cts.Token;
        //    var opt = (CallOptions)client.AsDynamic().option;
        //    opt.CancellationToken.IsNot(tk);

        //    var change = client.WithCancellationToken(tk);
        //    opt = (CallOptions)change.AsDynamic().option;

        //    opt.CancellationToken.Should().Be(tk);
        //}

        //[Fact]
        //public async Task WithDeadline()
        //{
        //    var now = DateTime.UtcNow.AddMinutes(10);
        //    var opt = (CallOptions)client.AsDynamic().option;
        //    opt.Deadline.IsNull();

        //    var change = client.WithDeadline(now);
        //    opt = (CallOptions)change.AsDynamic().option;
        //    opt.Deadline.Should().Be(now);

        //    var context = await change.ReturnContext();
        //    context.Deadline.ToString().Should().Be(now.ToString()); // almost same:)
        //}

        //[Fact]
        //public async Task WithHeaders()
        //{
        //    var meta = new Metadata();
        //    meta.Add("test", "aaaaa");

        //    var opt = (CallOptions)client.AsDynamic().option;
        //    opt.Headers.IsNull();

        //    var change = client.WithHeaders(meta);
        //    opt = (CallOptions)change.AsDynamic().option;
        //    opt.Headers[0].Key.Should().Be("test");
        //    opt.Headers[0].Value.Should().Be("aaaaa");

        //    var context = await change.ReturnContext();
        //    context.RequestHeaders["test"].Should().Be("aaaaa");
        //}


        //[Fact]
        //public void WithOption()
        //{
        //    var opt = (CallOptions)client.AsDynamic().option;
        //    opt.WriteOptions.IsNull();

        //    var change = client.WithOptions(new CallOptions(writeOptions: new WriteOptions(WriteFlags.BufferHint)));
        //    opt = (CallOptions)change.AsDynamic().option;
        //    opt.WriteOptions.Flags.Should().Be(WriteFlags.BufferHint);
        //}

    }
}
