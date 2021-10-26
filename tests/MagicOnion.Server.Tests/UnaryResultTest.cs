using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MagicOnion.Server.Tests
{
    public class UnaryResultTest
    {
        [Fact]
        public async Task FromResult()
        {
            (await UnaryResult.FromResult(123)).Should().Be(123);
            (await UnaryResult.FromResult("foo")).Should().Be("foo");
            (await UnaryResult.FromResult<string>(default(string))).Should().BeNull();

            Assert.Throws<ArgumentNullException>(() => UnaryResult.FromResult(default(Task<string>)));
        }

        [Fact]
        public async Task Ctor_RawValue()
        {
            var result = new UnaryResult<int>(123);
            (await result).Should().Be(123);

            var result2 = new UnaryResult<string>("foo");
            (await result2).Should().Be("foo");

            var result3 = new UnaryResult<string>(default(string));
            (await result3).Should().BeNull();
        }

        [Fact]
        public async Task Ctor_RawTask()
        {
            var result = new UnaryResult<int>(Task.FromResult(456));
            (await result).Should().Be(456);

            var result2 = new UnaryResult<string>(Task.FromResult("foo"));
            (await result2).Should().Be("foo");

            Assert.Throws<ArgumentNullException>(() => new UnaryResult<string>(default(Task<string>)));
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
}
