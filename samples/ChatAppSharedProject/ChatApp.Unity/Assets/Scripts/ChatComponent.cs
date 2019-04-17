using ChatApp.Shared;
using Grpc.Core;
using MagicOnion.Client;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ChatComponent : MonoBehaviour, IChatHubReceiver
    {
        private Channel channel;
        private IChatHub streamingClient;
        private IChatService client;

        private bool isJoin;

        public Text ChatText;

        public Button JoinOrLeaveButton;

        public Text JoinOrLeaveButtonText;

        public Button SendMessageButton;

        public InputField Input;

        public InputField ReportInput;


        void Start()
        {
            this.InitializeClient();
            this.InitializeUi();
        }


        async void OnDestroy()
        {
            //Clean up Hub and channel
            await this.streamingClient.DisposeAsync();
            await this.channel.ShutdownAsync();
        }


        private void InitializeClient()
        {
            // Initialize the Hub
            this.channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
            this.streamingClient = StreamingHubClient.Connect<IChatHub, IChatHubReceiver>(this.channel, this);
            this.client = MagicOnionClient.Create<IChatService>(this.channel);
        }


        private void InitializeUi()
        {
            this.isJoin = false;

            this.SendMessageButton.gameObject.SetActive(false);
            this.ChatText.text = string.Empty;
            this.Input.text = string.Empty;
            this.Input.placeholder.GetComponent<Text>().text = "Please enter your name.";
            this.JoinOrLeaveButtonText.text = "Enter the room";
        }


        #region Client -> Server (Streaming)
        public async void JoinOrLeave()
        {
            if (this.isJoin)
            {
                await this.streamingClient.LeaveAsync();

                this.InitializeUi();
            }
            else
            {
                var request = new JoinRequest { RoomName = "SampleRoom", UserName = this.Input.text };
                await this.streamingClient.JoinAsync(request);

                this.isJoin = true;
                this.SendMessageButton.gameObject.SetActive(true);
                this.JoinOrLeaveButtonText.text = "Leave the room";
                this.Input.text = string.Empty;
                this.Input.placeholder.GetComponent<Text>().text = "Please enter a comment.";
            }
        }


        public async void SendMessage()
        {
            if (!this.isJoin)
                return;

            await this.streamingClient.SendMessageAsync(this.Input.text);

            this.Input.text = string.Empty;
        }
        #endregion


        #region Server -> Client (Streaming)
        public void OnJoin(string name)
        {
            this.ChatText.text += $"\n<color=grey>{name} entered the room.</color>";
        }


        public void OnLeave(string name)
        {
            this.ChatText.text += $"\n<color=grey>{name} left the room.</color>";
        }

        public void OnSendMessage(MessageResponse message)
        {
            this.ChatText.text += $"\n{message.UserName}：{message.Message}";
        }
        #endregion


        #region Client -> Server
        public async void SendReport()
        {
            await this.client.SendReportAsync(this.ReportInput.text);

            this.ReportInput.text = string.Empty;
        }
        #endregion
    }
}
