using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandbox
{
    [MessagePackObject]
    public struct RoomMember
    {
        [Key(0)]
        public readonly string Id;
        [Key(1)]
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

    [MessagePackObject]
    public class ChatMessage
    {
        [Key(0)]
        public virtual RoomMember Sender { get; set; }
        [Key(1)]
        public virtual string Message { get; set; }

        public override string ToString()
        {
            return Sender.Name + ": " + Message;
        }
    }

    [MessagePackObject]
    public class ChatRoomResponse
    {
        [Key(0)]
        public virtual string Id { get; set; }
        [Key(1)]
        public virtual string Name { get; set; }

        public override string ToString()
        {
            return Id + ":" + Name;
        }
    }
}
