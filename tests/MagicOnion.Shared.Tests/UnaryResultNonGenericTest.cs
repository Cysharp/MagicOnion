using MagicOnion.Client;

namespace MagicOnion.Shared.Tests;

public class UnaryResultNonGenericTest
{
    [Fact]
    public async Task Ctor_Default()
    {
        var result = default(UnaryResult);
        await result;
    }

    [Fact]
    public async Task Ctor_RawValue()
    {
        var result = new UnaryResult(Nil.Default);
        await result;
    }

    [Fact]
    public async Task Ctor_RawTask()
    {
        var result = new UnaryResult(Task.FromResult(Nil.Default));
        await result;
    }

    [Fact]
    public void Ctor_RawTask_Null()
    {
        // Arrange
        var value = default(Task<Nil>);
        // Act
        var result = Record.Exception(() => new UnaryResult(value));
        // Assert
        result.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task Ctor_TaskOfResponseContext()
    {
        var result = new UnaryResult(Task.FromResult(DummyResponseContext.Create(Nil.Default)));
        await result;
    }

    [Fact]
    public void Ctor_TaskOfResponseContext_Null()
    {
        // Arrange
        var value = default(Task<IResponseContext<Nil>>);
        // Act
        var result = Record.Exception(() => new UnaryResult(value));
        // Assert
        result.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task ResponseHeadersAsync_Ctor_ResponseContext()
    {
        var result = new UnaryResult(Task.FromResult<IResponseContext<Nil>>(new DummyResponseContext<Nil>(Nil.Default)));
        (await result.ResponseHeadersAsync).Should().Contain(x => x.Key == "x-foo-bar" && x.Value == "baz");
    }

    [Fact]
    public async Task ResponseHeadersAsync_Never_Ctor_ResponseContext()
    {
        var result = new UnaryResult(Task.FromResult<IResponseContext<Nil>>(new DummyResponseContext<Nil>(new TaskCompletionSource<Nil>())));
        await Assert.ThrowsAsync<TimeoutException>(async () => await result.ResponseHeadersAsync.WaitAsync(TimeSpan.FromMilliseconds(250)));
    }

    [Fact]
    public async Task ResponseAsync_Ctor_Task()
    {
        var result = new UnaryResult(Task.FromResult(Nil.Default));
        await result.ResponseAsync;
    }

    [Fact]
    public async Task ResponseAsync_Ctor_ResponseContext()
    {
        var result = new UnaryResult(Task.FromResult<IResponseContext<Nil>>(new DummyResponseContext<Nil>(Nil.Default)));
        await result.ResponseAsync;
    }

    [Fact]
    public async Task ResponseAsync_Never_Ctor_Task()
    {
        var result = new UnaryResult(new TaskCompletionSource<Nil>().Task);
        await Assert.ThrowsAsync<TimeoutException>(async () => await result.ResponseAsync.WaitAsync(TimeSpan.FromMilliseconds(250)));
    }

    [Fact]
    public async Task ResponseAsync_Never_Ctor_ResponseContext()
    {
        var result = new UnaryResult(Task.FromResult<IResponseContext<Nil>>(new DummyResponseContext<Nil>(new TaskCompletionSource<Nil>())));
        await Assert.ThrowsAsync<TimeoutException>(async () => await result.ResponseAsync.WaitAsync(TimeSpan.FromMilliseconds(250)));
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

        public Task<Metadata> ResponseHeadersAsync => task.ContinueWith(_ => new Metadata() { {"x-foo-bar", "baz"} });
        public Status GetStatus() => Status.DefaultSuccess;
        public Metadata GetTrailers() => Metadata.Empty;

        public Type ResponseType => typeof(T);
        public Task<T> ResponseAsync => task;

        public void Dispose() { }

    }
}
