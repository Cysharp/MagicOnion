using MessagePack;

namespace Assets.Scripts.ServerShared.MessagePackObjects
{
    /// <summary>
    /// Room participation information
    /// </summary>
    [MessagePackObject]
    public struct JoinRequest
    {
        [Key(0)]
        public string RoomName { get; set; }

        [Key(1)]
        public string UserName { get; set; }
    }
}
