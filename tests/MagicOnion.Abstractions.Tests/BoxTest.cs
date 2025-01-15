using MagicOnion.Internal;

namespace MagicOnion.Abstractions.Tests;

public class BoxTest
{
    [Fact]
    public void Equality()
    {
        var box1 = Box.Create(1);
        var box1a = Box.Create(1);
        var box2 = Box.Create(2);
        var box2a = Box.Create(2);

        Assert.True(box1.Equals(box1));
        Assert.True(box1.Equals(box1a));
        Assert.False(box1.Equals(box2));

        Assert.True(box2.Equals(box2));
        Assert.True(box2.Equals(box2a));

        Assert.False(box1.Equals(null!));
        Assert.False(box2.Equals(null!));

        Assert.False((default(Box<int>)! == box1!));
        Assert.False((box1! == default(Box<int>)!));
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
        Assert.Equal(box2.Value, box1.Value);
        Assert.Same(box2, box1);
    }
    
    [Fact]
    public void CacheBool()
    {
        // Act
        var box1 = Box.Create(true);
        var box2 = Box.Create(false);

        // Assert
        Assert.True(box1.Value);
        Assert.False(box2.Value);
        Assert.NotEqual(box2.Value, box1.Value);
        Assert.NotSame(box2, box1);
    }

    [Fact]
    public void CacheBoolTrue()
    {
        // Act
        var box1 = Box.Create(true);
        var box2 = Box.Create(true);

        // Assert
        Assert.True(box1.Value);
        Assert.Equal(box2.Value, box1.Value);
        Assert.Same(box2, box1);
    }
    
    [Fact]
    public void CacheBoolFalse()
    {
        // Act
        var box1 = Box.Create(false);
        var box2 = Box.Create(false);

        // Assert
        Assert.False(box1.Value);
        Assert.Equal(box2.Value, box1.Value);
        Assert.Same(box2, box1);
    }
}
