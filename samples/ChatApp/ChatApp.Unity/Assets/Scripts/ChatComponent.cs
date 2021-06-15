using System;
using ChatApp.Shared.Hubs;
using ChatApp.Shared.MessagePackObjects;
using ChatApp.Shared.Services;
using Grpc.Core;
using MagicOnion.Client;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ChatComponent : MonoBehaviour, IChatHubReceiver
    {
        private CancellationTokenSource shutdownCancellation = new CancellationTokenSource();
        private ChannelBase channel;
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
        public Button ExceptionButton;
        public Button UnaryExceptionButton;


        async void Start()
        {
            await this.InitializeClientAsync();
            this.InitializeUi();
        }


        async void OnDestroy()
        {
            // Clean up Hub and channel
            shutdownCancellation.Cancel();
            
            if (this.streamingClient != null) await this.streamingClient.DisposeAsync();
            if (this.channel != null) await this.channel.ShutdownAsync();
        }


        private async Task InitializeClientAsync()
        {
            // Initialize the Hub
            // NOTE: If you want to use SSL/TLS connection, see InitialSettings.OnRuntimeInitialize method.
            this.channel = GrpcChannelx.ForAddress("http://localhost:5000");

            while (!shutdownCancellation.IsCancellationRequested)
            {
                try
                {
                    Debug.Log($"Connecting to the server...");
                    this.streamingClient = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(this.channel, this, cancellationToken: shutdownCancellation.Token);
                    this.RegisterDisconnectEvent(streamingClient);
                    Debug.Log($"Connection is established.");
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                Debug.Log($"Failed to connect to the server. Retry after 5 seconds...");
                await Task.Delay(5 * 1000);
            }

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
            this.ExceptionButton.interactable = false;
        }


        private async void RegisterDisconnectEvent(IChatHub streamingClient)
        {
            try
            {
                // you can wait disconnected event
                await streamingClient.WaitForDisconnect();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                // try-to-reconnect? logging event? close? etc...
                Debug.Log($"disconnected from the server.");

                if (this.isSelfDisConnected)
                {
                    // there is no particular meaning
                    await Task.Delay(2000);

                    // reconnect
                    await this.ReconnectServerAsync();
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
            this.ExceptionButton.interactable = false;
            this.UnaryExceptionButton.interactable = false;

            if (this.isJoin)
                this.JoinOrLeave();

            await this.streamingClient.DisposeAsync();
        }

        public async void ReconnectInitializedServer()
        {
            if (this.channel != null)
            {
                var chan = this.channel;
                if (chan == Interlocked.CompareExchange(ref this.channel, null, chan))
                {
                    await chan.ShutdownAsync();
                    this.channel = null;
                }
            }
            if (this.streamingClient != null)
            {
                var streamClient = this.streamingClient;
                if (streamClient == Interlocked.CompareExchange(ref this.streamingClient, null, streamClient))
                {
                    await streamClient.DisposeAsync();
                    this.streamingClient = null;
                }
            }

            if (this.channel == null && this.streamingClient == null)
            {
                await this.InitializeClientAsync();
                this.InitializeUi();
            }
        }


        private async Task ReconnectServerAsync()
        {
            Debug.Log($"Reconnecting to the server...");
            this.streamingClient = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(this.channel, this);
            this.RegisterDisconnectEvent(streamingClient);
            Debug.Log("Reconnected.");

            this.JoinOrLeaveButton.interactable = true;
            this.SendMessageButton.interactable = false;
            this.SendReportButton.interactable = true;
            this.DisconnectButon.interactable = true;
            this.ExceptionButton.interactable = true;
            this.UnaryExceptionButton.interactable = true;

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
                this.ExceptionButton.interactable = true;
            }
        }


        public async void SendMessage()
        {
            if (!this.isJoin)
                return;

            await this.streamingClient.SendMessageAsync(this.Input.text);

            this.Input.text = string.Empty;
        }

        public async void GenerateException()
        {
            // hub
            if (!this.isJoin) return;
            await this.streamingClient.GenerateException("client exception(streaminghub)!");
        }

        public void SampleMethod()
        {
            throw new System.NotImplementedException();
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


        #region Client -> Server (Unary)
        public async void SendReport()
        {
            await this.client.SendReportAsync(this.ReportInput.text);

            this.ReportInput.text = string.Empty;
        }

        public async void UnaryGenerateException()
        {
            // unary
            await this.client.GenerateException("client exception(unary)！");
        }
        #endregion
    }
}
