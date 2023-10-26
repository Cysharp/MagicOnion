using MagicOnion.Client;

namespace MagicOnion.Shared.Tests;

public class UnaryResultTest
{
    [Fact]
    public async Task FromResult()
    {
        (await UnaryResult.FromResult(123)).Should().Be(123);
        (await UnaryResult.FromResult("foo")).Should().Be("foo");
        (await UnaryResult.FromResult<string?>(default(string))).Should().BeNull();

        Assert.Throws<ArgumentNullException>(() => UnaryResult.FromResult(default(Task<string>)!));
    }

    [Fact]
    public async Task Ctor_RawValue()
    {
        var result = new UnaryResult<int>(123);
        (await result).Should().Be(123);

        var result2 = new UnaryResult<string>("foo");
        (await result2).Should().Be("foo");

        var result3 = new UnaryResult<string?>(default(string));
        (await result3).Should().BeNull();
    }

    [Fact]
    public async Task Ctor_RawTask()
    {
        var result = new UnaryResult<int>(Task.FromResult(456));
        (await result).Should().Be(456);

        var result2 = new UnaryResult<string>(Task.FromResult("foo"));
        (await result2).Should().Be("foo");

        Assert.Throws<ArgumentNullException>(() => new UnaryResult<string?>(default(Task<string?>)!));
    }

    [Fact]
    public async Task Ctor_TaskOfResponseContext()
    {
        var result = new UnaryResult<int>(Task.FromResult(DummyResponseContext.Create(456)));
        (await result).Should().Be(456);

        var result2 = new UnaryResult<string>(Task.FromResult(DummyResponseContext.Create("foo")));
        (await result2).Should().Be("foo");

        Assert.Throws<ArgumentNullException>(() => new UnaryResult<string?>(default(Task<IResponseContext<string?>>)!));
    }

    static class DummyResponseContext
    {
        public static IResponseContext<T> Create<T>(T value)
            => new DummyResponseContext<T>(value);
    }
    class DummyResponseContext<T> : IResponseContext<T>
    {
        readonly T value;

        public DummyResponseContext(T value)
        {
            this.value = value;
        }

        public Task<Metadata> ResponseHeadersAsync => Task.FromResult(Metadata.Empty);
        public Status GetStatus() => Status.DefaultSuccess;
        public Metadata GetTrailers() => Metadata.Empty;

        public Type ResponseType => typeof(T);
        public Task<T> ResponseAsync => Task.FromResult(value);

        public void Dispose() { }

    }

    [Fact]
    public async Task Ctor_Default()
    {
        var result = default(UnaryResult<int>);
        (await result).Should().Be(0);

        var result2 = default(UnaryResult<string>);
        (await result2).Should().Be(null);
    }
}
