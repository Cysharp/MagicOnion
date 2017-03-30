using System;
using MessagePack;

namespace SharedLibrary
{
    [MessagePackObject]
    public class MyRequest
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Data { get; set; }
    }

    [MessagePackObject]
    public struct MyStructRequest
    {
        [Key(0)]
        public int X;
        [Key(1)]
        public int Y;

        public MyStructRequest(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [MessagePackObject]
    public struct MyStructResponse
    {
        [Key(0)]
        public int X;
        [Key(1)]
        public int Y;

        public MyStructResponse(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [MessagePackObject]
    public class MyResponse
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Data { get; set; }
    }

    [MessagePackObject]
    public class MyHugeResponse
    {
        [Key(0)]
        public int x { get; set; }
        [Key(1)]
        public int y { get; set; }
        [Key(2)]
        public string z { get; set; }
        [Key(3)]
        public MyEnum e { get; set; }
        [Key(4)]
        public MyStructResponse soho { get; set; }
        [Key(5)]
        public ulong zzz { get; set; }
        [Key(6)]
        public MyRequest req { get; set; }
    }

    public enum MyEnum
    {
        Apple, Orange, Grape
    }

    [MessagePackObject]
    public class MyClass2
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public int Sum { get; set; }
    }

    public enum MyEnum2
    {
        A = 2,
        B = 3,
        C = 4
    }
}