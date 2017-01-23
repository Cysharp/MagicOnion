using UnityEngine;
using RuntimeUnitTestToolkit;
using UniRx;
using System.Collections;
using Grpc.Core;
using MagicOnion.Client;
using Sandbox.ConsoleServer;
using ZeroFormatter.Formatters;
using System.Collections.Generic;
using System;

namespace MagicOnion.Tests
{
    public class HeartbeatTest
    {
        public IEnumerator Ping()
        {
            var channel = UnitTestClient.GetChannel();

            var ping = new MagicOnion.Client.EmbeddedServices.PingClient(channel).Ping().ResponseAsync.ToYieldInstruction();

            yield return ping;

            if (ping.HasError)
            {
                throw ping.Error;
            }
            else
            {
                Debug.Log("Client -> Server: " + ping.Result + "ms");
            }
        }

        public IEnumerator Heartbeat()
        {
            var channel = UnitTestClient.GetChannel();
            var context = new ChannelContext(channel);

            yield return context.WaitConnectComplete().ToYieldInstruction();


            yield return new WaitForSeconds(3);

            context.Dispose();
        }
    }
}