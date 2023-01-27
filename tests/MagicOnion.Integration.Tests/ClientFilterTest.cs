#pragma warning disable CS1998

using MagicOnion.Client;
using MagicOnion.Server;
using Xunit.Abstractions;
using System.Diagnostics;
using Grpc.Net.Client;
using MagicOnion.Integration.Tests.Generated;
using MagicOnion.Serialization;

namespace MagicOnion.Integration.Tests;

public class ClientFilterTest : IClassFixture<MagicOnionApplicationFactory<ClientFilterTestService>>
{
    readonly ITestOutputHelper logger;
    readonly MagicOnionApplicationFactory<ClientFilterTestService> factory;

    public ClientFilterTest(ITestOutputHelper logger, MagicOnionApplicationFactory<ClientFilterTestService> factory)
    {
        this.logger = logger;
        this.factory = factory;
    }

    public static IEnumerable<object[]> EnumerateMagicOnionClientFactory()
    {
        yield return new [] { new TestMagicOnionClientFactory("Dynamic", DynamicMagicOnionClientFactoryProvider.Instance) };
        yield return new [] { new TestMagicOnionClientFactory("Generated", MagicOnionGeneratedClientFactoryProvider.Instance) };
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task SimpleFilter(TestMagicOnionClientFactory clientFactory)
    {
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        for (int i = 1; i <= 20; i++)
        {
            var filters = Enumerable.Range(0, i).Select(_ => new CountFilter(i)).ToArray();
            var client = clientFactory.Create<IClientFilterTestService>(channel, filters);
            var r0 = await client.Unary1(1000, 2000);
            r0.Should().Be(3000 + 10 * i);

            foreach (var item in filters)
            {
                item.CalledCount.Should().Be(1);
            }
        }
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task HeaderEcho(TestMagicOnionClientFactory clientFactory)
    {
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var res = clientFactory.Create<IClientFilterTestService>(channel, new IClientFilter[]
        {
            new AppendHeaderFilter(),
            new RetryFilter()
        }).HeaderEcho();
        await res;

        var trailers = res.GetTrailers();
        trailers.Should().ContainSingle(x => x.Key == "x-foo" && x.Value == "abcdefg");
        trailers.Should().ContainSingle(x => x.Key == "x-bar" && x.Value == "hijklmn");
    }

    [Theory]
    [MemberData(nameof(EnumerateMagicOnionClientFactory))]
    public async Task ErrorRetry(TestMagicOnionClientFactory clientFactory)
    {
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var ex = await Assert.ThrowsAsync<RetryFailedException>(async () =>
        {
            var filter = new RetryFilter();
            await clientFactory.Create<IClientFilterTestService>(channel, new IClientFilter[]
            {
                filter
            }).AlwaysError();
        });

        ex.RetryCount.Should().Be(3);
        ex.LastException.Should().NotBeNull();
        logger.WriteLine(ex.LastException?.ToString());
    }
}
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

        return UnaryResult.FromResult(Nil.Default);
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
            return ResponseContext<int>.Create(9999, Status.DefaultSuccess, Metadata.Empty, Metadata.Empty);
        }

        return await next(context);
    }
}

public class RetryFilter : IClientFilter
{
    public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
    {
        Exception? lastException = null;
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
    public Exception? LastException { get; }

    public RetryFailedException(int retryCount, Exception? lastException)
    {
        this.RetryCount = retryCount;
        this.LastException = lastException;
    }
}
