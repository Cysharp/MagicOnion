using MagicOnion.Client;

namespace MagicOnion.Abstractions.Tests;

public class UnaryResultTest
{
    [Fact]
    public async Task FromResult()
    {
        Assert.Equal(123, (await UnaryResult.FromResult(123)));
        Assert.Equal("foo", (await UnaryResult.FromResult("foo")));
        Assert.Null((await UnaryResult.FromResult<string?>(default(string))));

        Assert.Throws<ArgumentNullException>(() => UnaryResult.FromResult(default(Task<string>)!));
    }

    [Fact]
    public async Task Ctor_RawValue()
    {
        var result = new UnaryResult<int>(123);
        Assert.Equal(123, (await result));

        var result2 = new UnaryResult<string>("foo");
        Assert.Equal("foo", (await result2));

        var result3 = new UnaryResult<string?>(default(string));
        Assert.Null((await result3));
    }

    [Fact]
    public async Task Ctor_RawTask()
    {
        var result = new UnaryResult<int>(Task.FromResult(456));
        Assert.Equal(456, (await result));

        var result2 = new UnaryResult<string>(Task.FromResult("foo"));
        Assert.Equal("foo", (await result2));

        Assert.Throws<ArgumentNullException>(() => new UnaryResult<string?>(default(Task<string?>)!));
    }

    [Fact]
    public async Task Ctor_TaskOfResponseContext()
    {
        var result = new UnaryResult<int>(Task.FromResult(DummyResponseContext.Create(456)));
        Assert.Equal(456, (await result));

        var result2 = new UnaryResult<string>(Task.FromResult(DummyResponseContext.Create("foo")));
        Assert.Equal("foo", (await result2));

        Assert.Throws<ArgumentNullException>(() => new UnaryResult<string?>(default(Task<IResponseContext<string?>>)!));
    }

    static class DummyResponseContext
    {
        public static IResponseContext<T> Create<T>(T value)
            => new DummyResponseContext<T>(value);
    }
    class DummyResponseContext<T> : IResponseContext<T>
    {
        readonly Task<T> task;

        public DummyResponseContext(T value)
        {
            this.task = Task.FromResult(value);
        }

        public DummyResponseContext(TaskCompletionSource<T> taskCompletionSource)
        {
            this.task = taskCompletionSource.Task;
        }

        public Task<Metadata> ResponseHeadersAsync => task.ContinueWith(_ => new Metadata() { { "x-foo-bar", "baz" } });
        public Status GetStatus() => Status.DefaultSuccess;
        public Metadata GetTrailers() => Metadata.Empty;

        public Type ResponseType => typeof(T);
        public Task<T> ResponseAsync => task;

        public void Dispose() { }

    }

    [Fact]
    public async Task Ctor_Default()
    {
        var result = default(UnaryResult<int>);
        Assert.Equal(0, (await result));

        var result2 = default(UnaryResult<string>);
        Assert.Equal(null, (await result2));
    }

    [Fact]
    public async Task ResponseHeadersAsync_Ctor_ResponseContext()
    {
        var result = new UnaryResult<int>(Task.FromResult<IResponseContext<int>>(new DummyResponseContext<int>(123)));
        Assert.Contains((await result.ResponseHeadersAsync), x => x.Key == "x-foo-bar" && x.Value == "baz");
    }

    [Fact]
    public async Task ResponseHeadersAsync_Never_Ctor_ResponseContext()
    {
        var result = new UnaryResult<int>(Task.FromResult<IResponseContext<int>>(new DummyResponseContext<int>(new TaskCompletionSource<int>())));
        await Assert.ThrowsAsync<TimeoutException>(async () => await result.ResponseHeadersAsync.WaitAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResponseAsync_Ctor_Task()
    {
        var result = new UnaryResult<int>(Task.FromResult(123));
        Assert.Equal(123, (await result.ResponseAsync));
    }

    [Fact]
    public async Task ResponseAsync_Ctor_TaskOfNil()
    {
        var result = new UnaryResult<int>(Task.FromResult(123));
        Assert.Equal(123, (await result.ResponseAsync));
    }

    [Fact]
    public async Task ResponseAsync_Ctor_ResponseContext()
    {
        var result = new UnaryResult<int>(Task.FromResult<IResponseContext<int>>(new DummyResponseContext<int>(123)));
        await result.ResponseAsync;
    }

    [Fact]
    public async Task ResponseAsync_Never_Ctor_Task()
    {
        var result = new UnaryResult<int>(new TaskCompletionSource<int>().Task);
        await Assert.ThrowsAsync<TimeoutException>(async () => await result.ResponseAsync.WaitAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken));
    }


    [Fact]
    public async Task ResponseAsync_Never_Ctor_ResponseContext()
    {
        var result = new UnaryResult<int>(Task.FromResult<IResponseContext<int>>(new DummyResponseContext<int>(new TaskCompletionSource<int>())));
        await Assert.ThrowsAsync<TimeoutException>(async () => await result.ResponseAsync.WaitAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken));
    }
}
