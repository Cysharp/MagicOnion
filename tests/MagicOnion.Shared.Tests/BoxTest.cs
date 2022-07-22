using MagicOnion.Internal;

namespace MagicOnion.Shared.Tests;

public class BoxTest
{
    [Fact]
    public void Equality()
    {
        var box1 = Box.Create(1);
        var box1a = Box.Create(1);
        var box2 = Box.Create(2);
        var box2a = Box.Create(2);

        box1.Equals(box1).Should().BeTrue();
        box1.Equals(box1a).Should().BeTrue();
        box1.Equals(box2).Should().BeFalse();

        box2.Equals(box2).Should().BeTrue();
        box2.Equals(box2a).Should().BeTrue();

        box1.Equals(null).Should().BeFalse();
        box2.Equals(null).Should().BeFalse();

        (default(Box<int>) == default(Box<int>)).Should().BeTrue();
        (default(Box<int>) == box1).Should().BeFalse();
        (box1 == default(Box<int>)).Should().BeFalse();
    }

    [Fact]
    public void CacheNil()
    {
        // Arrange
        var value = Nil.Default;

        // Act
        var box1 = Box.Create(value);
        var box2 = Box.Create(value);

        // Assert
        box1.Value.Should().Be(box2.Value);
        box1.Should().BeSameAs(box2);
    }
    
    [Fact]
    public void CacheBool()
    {
        // Act
        var box1 = Box.Create(true);
        var box2 = Box.Create(false);

        // Assert
        box1.Value.Should().BeTrue();
        box2.Value.Should().BeFalse();
        box1.Value.Should().NotBe(box2.Value);
        box1.Should().NotBeSameAs(box2);
    }

    [Fact]
    public void CacheBoolTrue()
    {
        // Act
        var box1 = Box.Create(true);
        var box2 = Box.Create(true);

        // Assert
        box1.Value.Should().BeTrue();
        box1.Value.Should().Be(box2.Value);
        box1.Should().BeSameAs(box2);
    }
    
    [Fact]
    public void CacheBoolFalse()
    {
        // Act
        var box1 = Box.Create(false);
        var box2 = Box.Create(false);

        // Assert
        box1.Value.Should().BeFalse();
        box1.Value.Should().Be(box2.Value);
        box1.Should().BeSameAs(box2);
    }
}
