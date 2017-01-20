using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroFormatter;

namespace Sandbox
{
    [ZeroFormattable]
    public struct RoomMember
    {
        [Index(0)]
        public readonly string Id;
        [Index(1)]
        public readonly string Name;

        public RoomMember(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Id + ":" + Name;
        }
    }

    [ZeroFormattable]
    public class ChatMessage
    {
        [Index(0)]
        public virtual RoomMember Sender { get; set; }
        [Index(1)]
        public virtual string Message { get; set; }

        public override string ToString()
        {
            return Sender.Name + ": " + Message;
        }
    }

    [ZeroFormattable]
    public class ChatRoomResponse
    {
        [Index(0)]
        public virtual string Id { get; set; }
        [Index(1)]
        public virtual string Name { get; set; }

        public override string ToString()
        {
            return Id + ":" + Name;
        }
    }
}
