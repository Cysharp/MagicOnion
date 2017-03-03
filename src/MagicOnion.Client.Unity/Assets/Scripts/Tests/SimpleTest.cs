using UnityEngine;
using RuntimeUnitTestToolkit;
using UniRx;
using System.Collections;
using Grpc.Core;
using MagicOnion.Client;
using Sandbox.ConsoleServer;
using System.Collections.Generic;
using System;

namespace MagicOnion.Tests
{
    public class SimpleTest
    {
        IMyFirstService GetClient()
        {
            return UnitTestClient.Create<IMyFirstService>();
        }

        public IEnumerator Unary()
        {
            var r = GetClient().SumAsync(1, 10).ResponseAsync.ToYieldInstruction();
            yield return r;

            r.Result.Is("11");
        }

        public IEnumerator ClientStreaming()
        {
            var client = GetClient().StreamingOne();

            yield return client.RequestStream.WriteAsync(1000).ToYieldInstruction();
            yield return client.RequestStream.WriteAsync(2000).ToYieldInstruction();
            yield return client.RequestStream.WriteAsync(3000).ToYieldInstruction();

            yield return client.RequestStream.CompleteAsync().ToYieldInstruction();

            var r = client.ResponseAsync.ToYieldInstruction();
            yield return r;

            r.Result.Is("finished");
        }

        public IEnumerator ServerStreaming()
        {
            var client = GetClient().StreamingTwo(10, 20, 3);

            var list = new List<string>();
            yield return client.ResponseStream.ForEachAsync(x =>
            {
                list.Add(x);
            }).ToYieldInstruction();

            list.IsCollection("30", "60", "90");
        }

        public IEnumerator DuplexStreaming()
        {
            var client = GetClient().StreamingThree();

            var l = new List<string>();
            var responseAwaiter = client.ResponseStream.ForEachAsync(x =>
            {
                l.Add(x);
            })
            .ToYieldInstruction();

            yield return client.RequestStream.WriteAsync(1).ToYieldInstruction();
            yield return client.RequestStream.WriteAsync(2).ToYieldInstruction();
            yield return client.RequestStream.WriteAsync(3).ToYieldInstruction();

            yield return client.RequestStream.CompleteAsync().ToYieldInstruction();

            yield return responseAwaiter;

            l.IsCollection("test1", "test2", "finish");
        }
    }
}