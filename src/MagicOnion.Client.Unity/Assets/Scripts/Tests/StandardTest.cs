using UnityEngine;
using RuntimeUnitTestToolkit;
using UniRx;
using System.Collections;
using Grpc.Core;
using MagicOnion.Client;
using Sandbox.ConsoleServer;
using ZeroFormatter.Formatters;
using System.Collections.Generic;

namespace MagicOnion.Tests
{
    public class StandardTest
    {
        IStandard GetClient()
        {
            return UnitTestClient.Create<IStandard>();
        }

        public IEnumerator Unary()
        {
            var r = GetClient().Unary1Async(999, 3000).ResponseAsync.ToYieldInstruction();
            yield return r;

            r.Result.Is(3999);
        }

        public IEnumerator ClientStreaming()
        {
            var client = GetClient().ClientStreaming1Async();

            yield return client.RequestStream.WriteAsync(1000).ToYieldInstruction();
            yield return client.RequestStream.WriteAsync(2000).ToYieldInstruction();
            yield return client.RequestStream.WriteAsync(3000).ToYieldInstruction();

            yield return client.RequestStream.CompleteAsync().ToYieldInstruction();

            var r = client.ResponseAsync.ToYieldInstruction();
            yield return r;

            r.Result.Is("1000, 2000, 3000");
        }

        public IEnumerator ServerStreaming()
        {
            var client = GetClient().ServerStreamingAsync(10, 20, 3);

            var list = new List<int>();
            yield return client.ResponseStream.ForEachAsync(x =>
            {
                list.Add(x);
            }).ToYieldInstruction();

            list.IsCollection(30, 30, 30);
        }

        public IEnumerator DuplexStreaming()
        {
            var client = GetClient().DuplexStreamingAsync();

            var l = new List<int>();
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

            l.IsCollection(1, 2, 3);
        }
    }
}