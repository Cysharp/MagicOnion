using MessagePack;

namespace Assets.Scripts.ServerShared.MessagePackObjects
{
    /// <summary>
    /// Message information
    /// </summary>
    [MessagePackObject]
    public struct MessageResponse
    {
        [Key(0)]
        public string UserName { get; set; }

        [Key(1)]
        public string Message { get; set; }
    }
}
