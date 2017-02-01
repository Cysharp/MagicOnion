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
    public class MetadataTest
    {
        public IEnumerator Pomu()
        {
            var channel = UnitTestClient.GetChannel();

            var client = MagicOnionClient.Create<ISendMetadata>(channel);

            var pongpong = client.PangPong();

            var res = pongpong.ResponseAsync.ToYieldInstruction();
            yield return res;

            if (res.HasError)
            {
                Debug.Log("Error:" + res.Error.ToString());
            }
            else
            {
                Debug.Log("No Error");
            }

            channel.ShutdownAsync().Subscribe();
        }

        //static Channel _staticChannel;
        //static ChannelContext _ctx;

        //public IEnumerator Humo()
        //{
        //    _staticChannel = UnitTestClient.GetChannel();
        //    _ctx = new ChannelContext(_staticChannel);
        //    yield return _ctx.WaitConnectComplete().ToYieldInstruction();
        //}
    }
}