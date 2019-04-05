using Assets.Scripts.ServerShared.MessagePackObjects;

namespace Assets.Scripts.ServerShared.Hubs
{
    /// <summary>
    /// Server -> Client API
    /// </summary>
    public interface IChatHubReceiver
    {
        void OnJoin(string name);

        void OnLeave(string name);

        void OnSendMessage(MessageResponse message);
    }
}
