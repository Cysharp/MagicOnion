using UnityEngine;
using UniRx;
using System.Collections;
using Grpc.Core;

namespace MagicOinon.Tests
{
    public class ManualTest
    {

        public IEnumerator Hoge()
        {
            var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
            yield return channel.ConnectAsync().ToYieldInstruction();

        }
    }
}