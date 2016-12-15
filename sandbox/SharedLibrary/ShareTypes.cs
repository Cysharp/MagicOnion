using System;
using ZeroFormatter;

namespace SharedLibrary
{
    [ZeroFormattable]
    public class MyRequest
    {
        [Index(0)]
        public virtual int Id { get; set; }
        [Index(1)]
        public virtual string Data { get; set; }
    }

    [ZeroFormattable]
    public struct MyStructRequest
    {
        [Index(0)]
        public int X;
        [Index(1)]
        public int Y;

        public MyStructRequest(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [ZeroFormattable]
    public struct MyStructResponse
    {
        [Index(0)]
        public int X;
        [Index(1)]
        public int Y;

        public MyStructResponse(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary>
    /// Represents Void/Unit.
    /// </summary>
    [ZeroFormattable]
    public struct Nil : IEquatable<Nil>
    {
        public static readonly Nil Default = default(Nil);

        public bool Equals(Nil other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    [ZeroFormattable]
    public class MyResponse
    {
        [Index(0)]
        public virtual int Id { get; set; }
        [Index(1)]
        public virtual string Data { get; set; }
    }

    [ZeroFormattable]
    public class MyHugeResponse
    {
        [Index(0)]
        public virtual int x { get; set; }
        [Index(1)]
        public virtual int y { get; set; }
        [Index(2)]
        public virtual string z { get; set; }
        [Index(3)]
        public virtual MyEnum e { get; set; }
        [Index(4)]
        public virtual MyStructResponse soho { get; set; }
        [Index(5)]
        public virtual ulong zzz { get; set; }
        [Index(6)]
        public virtual MyRequest req { get; set; }
    }

    public enum MyEnum
    {
        Apple, Orange, Grape
    }
}