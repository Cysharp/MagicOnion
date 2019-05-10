using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BugReproductionScene : MonoBehaviour, IBugReproductionHubReceiver
{
    private Channel channel;
    private IBugReproductionHub client;

    public Button Button;

    async void Start()
    {
        this.channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
        this.client = StreamingHubClient.Connect<IBugReproductionHub, IBugReproductionHubReceiver>(this.channel, this);

        await this.client.JoinAsync();

        this.Button.onClick.AddListener(CallAsync);
    }

    public async void CallAsync()
    {
        await this.client.CallAsync();
    }

    public void OnCall()
    {
        Debug.Log("OnCall!!");
    }
}

public interface IBugReproductionHubReceiver
{
    void OnCall();
}

public interface IBugReproductionHub : IStreamingHub<IBugReproductionHub, IBugReproductionHubReceiver>
{
    Task JoinAsync();

    Task CallAsync();
}
