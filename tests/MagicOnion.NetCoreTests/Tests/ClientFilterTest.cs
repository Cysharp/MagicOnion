#pragma warning disable CS1998

using Grpc.Core;
using FluentAssertions;
using MagicOnion.Client;
using MagicOnion.Server;
using MagicOnion.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Diagnostics;
using MessagePack;

namespace MagicOnion.NetCoreTests.Tests
{

    public interface IClientFilterTestService : IService<IClientFilterTestService>
    {
        UnaryResult<int> Unary1(int x, int y);
        UnaryResult<Nil> HeaderEcho();
        UnaryResult<Nil> AlwaysError();
    }

    public class ClientFilterTestService : ServiceBase<IClientFilterTestService>, IClientFilterTestService
    {

        public async UnaryResult<int> Unary1(int x, int y)
        {
            return x + y;
        }

        public UnaryResult<Nil> HeaderEcho()
        {
            foreach (var item in Context.CallContext.RequestHeaders)
            {
                Context.CallContext.ResponseTrailers.Add(item);
            }

            return ReturnNil();
        }

        public UnaryResult<Nil> AlwaysError()
        {
            throw new Exception("Ok, throw error!");
        }
    }

    public class CountFilter : IClientFilter
    {
        public int CalledCount;

        public int FilterCount;

        public CountFilter(int count)
        {
            this.FilterCount = count;
        }

        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            CalledCount++;
            var response = (await next(context)).As<int>();
            var newResult = await response.ResponseAsync;
            return response.WithNewResult(newResult + 10);
        }
    }

    public class AppendHeaderFilter : IClientFilter
    {
        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            var header = context.CallOptions.Headers;
            header.Add("x-foo", "abcdefg");
            header.Add("x-bar", "hijklmn");

            var response = await next(context);
            return response;
        }
    }

    public class LoggingFilter : IClientFilter
    {
        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            Console.WriteLine("Request Begin:" + context.MethodPath);

            var sw = Stopwatch.StartNew();
            var response = await next(context);
            sw.Stop();

            Console.WriteLine("Request Completed:" + context.MethodPath + ", Elapsed:" + sw.Elapsed.TotalMilliseconds + "ms");

            return response;
        }
    }

    public class ResponseHandlingFilter : IClientFilter
    {
        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            var response = await next(context);

            if (context.MethodPath == "ICalc/Sum")
            {
                var sumResult = await response.GetResponseAs<int>();
                Console.WriteLine("Called Sum, Result:" + sumResult);
            }

            return response;
        }
    }

    public class MockRequestFilter : IClientFilter
    {
        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            if (context.MethodPath == "ICalc/Sum")
            {
                // don't call next, return mock result.
                return new ResponseContext<int>(9999);
            }

            return await next(context);
        }
    }

    public class RetryFilter : IClientFilter
    {
        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            Exception lastException = null;
            var retryCount = 0;
            while (retryCount != 3)
            {
                try
                {
                    // using same CallOptions so be careful to add duplicate headers or etc.
                    return await next(context);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
                retryCount++;
            }

            throw new RetryFailedException(retryCount, lastException);
        }
    }


    public class RetryFailedException : Exception
    {
        public int RetryCount { get; }
        public Exception LastException { get; }

        public RetryFailedException(int retryCount, Exception lastException)
        {
            this.RetryCount = retryCount;
            this.LastException = lastException;
        }
    }

    [CollectionDefinition(nameof(ClientFilterTestCollectionFixture))]
    public class ClientFilterTestCollectionFixture : ICollectionFixture<ClientFilterTestCollectionFixture.CustomServerFixture>
    {
        public class CustomServerFixture : ServerFixture
        {
            protected override MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
            {
                return MagicOnionEngine.BuildServerServiceDefinition(new[] { typeof(ClientFilterTestService) }, options);
            }
        }
    }

    [Collection(nameof(ClientFilterTestCollectionFixture))]
    public class ClientFilterTest
    {
        ITestOutputHelper logger;
        Channel channel;

        public ClientFilterTest(ITestOutputHelper logger, ClientFilterTestCollectionFixture.CustomServerFixture server)
        {
            this.logger = logger;
            this.channel = server.DefaultChannel;
        }

        [Fact]
        public async Task SimpleFilter()
        {
            for (int i = 1; i <= 20; i++)
            {
                var filters = Enumerable.Range(0, i).Select(_ => new CountFilter(i)).ToArray();
                var client = MagicOnionClient.Create<IClientFilterTestService>(channel, filters);
                var r0 = await client.Unary1(1000, 2000);
                r0.Should().Be(3000 + 10 * i);

                foreach (var item in filters)
                {
                    item.CalledCount.Should().Be(1);
                }
            }
        }

        [Fact]
        public async Task HeaderEcho()
        {
            var res = MagicOnionClient.Create<IClientFilterTestService>(channel, new IClientFilter[]
            {
                new AppendHeaderFilter(),
                new RetryFilter()
            }).HeaderEcho();
            await res;
            var meta1 = res.GetTrailers()[0];
            meta1.Key.Should().Be("x-foo");
            meta1.Value.Should().Be("abcdefg");
            var meta2 = res.GetTrailers()[1];
            meta2.Key.Should().Be("x-bar");
            meta2.Value.Should().Be("hijklmn");
        }

        [Fact]
        public async Task ErrorRetry()
        {
            var ex = await Assert.ThrowsAsync<RetryFailedException>(async () =>
            {
                var filter = new RetryFilter();
                await MagicOnionClient.Create<IClientFilterTestService>(channel, new IClientFilter[]
                {
                    filter
                }).AlwaysError();
            });

            ex.RetryCount.Should().Be(3);
            logger.WriteLine(ex.LastException.ToString());
        }
    }
}
