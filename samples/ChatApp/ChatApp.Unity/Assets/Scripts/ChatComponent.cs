using ChatApp.Shared.Hubs;
using ChatApp.Shared.MessagePackObjects;
using ChatApp.Shared.Services;
using Grpc.Core;
using MagicOnion.Client;
using System.IO;
using System.Threading.Tasks;
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
        private bool isSelfDisConnected;

        public Text ChatText;

        public Button JoinOrLeaveButton;

        public Text JoinOrLeaveButtonText;

        public Button SendMessageButton;

        public InputField Input;

        public InputField ReportInput;

        public Button SendReportButton;

        public Button DisconnectButon;


        void Start()
        {
            this.InitializeClient();
            this.InitializeUi();
        }


        async void OnDestroy()
        {
            // Clean up Hub and channel
            await this.streamingClient.DisposeAsync();
            await this.channel.ShutdownAsync();
        }


        private void InitializeClient()
        {
            // Initialize the Hub
            this.channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
            // for SSL/TLS connection
            //var serverCred = new SslCredentials(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "server.crt")));
            //this.channel = new Channel("test.example.com", 12345, serverCred);
            this.streamingClient = StreamingHubClient.Connect<IChatHub, IChatHubReceiver>(this.channel, this);
            this.RegisterDisconnectEvent(streamingClient);
            this.client = MagicOnionClient.Create<IChatService>(this.channel);
        }


        private void InitializeUi()
        {
            this.isJoin = false;

            this.SendMessageButton.interactable = false;
            this.ChatText.text = string.Empty;
            this.Input.text = string.Empty;
            this.Input.placeholder.GetComponent<Text>().text = "Please enter your name.";
            this.JoinOrLeaveButtonText.text = "Enter the room";
        }


        private async void RegisterDisconnectEvent(IChatHub streamingClient)
        {
            try
            {
                // you can wait disconnected event
                await streamingClient.WaitForDisconnect();
            }
            finally
            {
                // try-to-reconnect? logging event? close? etc...
                Debug.Log("disconnected server.");

                if (this.isSelfDisConnected)
                {
                    // there is no particular meaning
                    await Task.Delay(2000);

                    // reconnect
                    this.ReconnectServer();
                }
            }
        }


        public async void DisconnectServer()
        {
            this.isSelfDisConnected = true;

            this.JoinOrLeaveButton.interactable = false;
            this.SendMessageButton.interactable = false;
            this.SendReportButton.interactable = false;
            this.DisconnectButon.interactable = false;

            if (this.isJoin)
                this.JoinOrLeave();

            await this.streamingClient.DisposeAsync();
        }


        private void ReconnectServer()
        {
            this.streamingClient = StreamingHubClient.Connect<IChatHub, IChatHubReceiver>(this.channel, this);
            this.RegisterDisconnectEvent(streamingClient);
            Debug.Log("Reconnected server.");

            this.JoinOrLeaveButton.interactable = true;
            this.SendMessageButton.interactable = false;
            this.SendReportButton.interactable = true;
            this.DisconnectButon.interactable = true;

            this.isSelfDisConnected = false;
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
                this.SendMessageButton.interactable = true;
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
