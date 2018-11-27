using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestExecution : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Do();
    }

    async void Do()
    {
        await new ClientProgram().Start("hoge", "huga");
    }

    // Update is called once per frame
    private void OnDestroy()
    {
    }
}

public class ClientProgram : IMessageReceiver
{
    public async Task Start(string user, string room)
    {
        Debug.Log("Start Create Channel");
        var channel = new Channel("localhost:12345", ChannelCredentials.Insecure);

        Debug.Log("Start Connect Channel");
        var client = StreamingHubClient.Connect<IChatHub, IMessageReceiver>(channel, this);
        // RegisterDisconnect(client);
        try
        {
            Debug.Log("Start Join");

            await client.JoinAsync(user, room);

            Debug.Log("Start Send");
            await client.SendMessageAsync("Who");
            await client.SendMessageAsync("Bar");
            await client.SendMessageAsync("Baz");

            await client.LeaveAsync();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            await client.DisposeAsync();
        }
    }

    async void RegisterDisconnect(IChatHub client)
    {
        try
        {
            // you can wait disconnected event
            await client.WaitForDisconnect();
        }
        finally
        {
            // try-to-reconnect? logging event? etc...
            Debug.Log("disconnected");
        }
    }

#pragma warning disable CS1998

    public async Task OnReceiveMessage(string senderUser, string message)
    {
        Debug.Log(senderUser + ":" + message);
    }
}

public interface IMessageReceiver
{
    Task OnReceiveMessage(string senderUser, string message);
}

public interface IChatHub : IStreamingHub<IChatHub, IMessageReceiver>
{
    Task<Nil> JoinAsync(string userName, string roomName);
    Task<Nil> LeaveAsync();
    Task SendMessageAsync(string message);
}