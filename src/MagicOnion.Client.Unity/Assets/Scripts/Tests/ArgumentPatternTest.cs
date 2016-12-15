using UnityEngine;
using RuntimeUnitTestToolkit;
using UniRx;
using System.Collections;
using Grpc.Core;
using MagicOnion.Client;
using Sandbox.ConsoleServer;
using ZeroFormatter.Formatters;
using System.Collections.Generic;
using SharedLibrary;
using System;

namespace MagicOnion.Tests
{
    public class ArgumentPatternTest
    {
        IArgumentPattern GetClient()
        {
            var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
            var client = MagicOnionClient.Create<IArgumentPattern>(channel);
            return client.WithDeadline(DateTime.UtcNow.AddSeconds(10));
        }

        public IEnumerator Unary1()
        {
            var r = GetClient()
                .Unary1(300, 200, "hogehoge", SharedLibrary.MyEnum.Grape,
                    new SharedLibrary.MyStructResponse(999, 9999), 4,
                    new SharedLibrary.MyRequest
                    {
                        Id = 3232,
                        Data = "hugahuga"
                    })
                .ResponseAsync.ToYieldInstruction();

            yield return r;

            var res = r.Result;
            res.x.Is(300);
            res.y.Is(200);
            res.z.Is("hogehoge");
            res.e.Is(SharedLibrary.MyEnum.Grape);
            res.soho.X.Is(999);
            res.soho.Y.Is(9999);
            res.zzz.Is((ulong)4);
            res.req.Id.Is(3232);
            res.req.Data.Is("hugahuga");
        }


        public IEnumerator Unary2()
        {
            var r = GetClient()
                .Unary2(
                    new SharedLibrary.MyRequest
                    {
                        Id = 3232,
                        Data = "hugahuga"
                    })
                .ResponseAsync.ToYieldInstruction();

            yield return r;

            var res = r.Result;
            res.Id.Is(3232);
            res.Data.Is("hugahuga");
        }



        public IEnumerator Unary3()
        {
            var r = GetClient()
                .Unary3()
                .ResponseAsync.ToYieldInstruction();

            yield return r;

            var res = r.Result;
            res.Id.Is(-1);
            res.Data.Is("empty");
        }



        public IEnumerator Unary5()
        {
            var r = GetClient()
                .Unary5(new SharedLibrary.MyStructRequest(300, 400))
                .ResponseAsync.ToYieldInstruction();

            yield return r;

            var res = r.Result;
            res.X.Is(300);
            res.Y.Is(400);
        }



        public IEnumerator ServerStreamingResult1()
        {
            MyResponse res = null;
            yield return GetClient()
                .ServerStreamingResult1(3, 4, "5")
                .ResponseStream
                .ForEachAsync(x => res = x)
                .ToYieldInstruction();

            res.Id.Is(7);
            res.Data.Is("5");
        }



        public IEnumerator ServerStreamingResult2()
        {
            MyResponse res = null;
            yield return GetClient()
                .ServerStreamingResult2(new SharedLibrary.MyRequest { Id = 32, Data = "dt" })
                .ResponseStream
                .ForEachAsync(x => res = x)
                .ToYieldInstruction();

            res.Id.Is(32);
            res.Data.Is("dt");
        }



        public IEnumerator ServerStreamingResult3()
        {
            MyResponse res = null;
            yield return GetClient()
                .ServerStreamingResult3()
                .ResponseStream
                .ForEachAsync(x => res = x)
                .ToYieldInstruction();

            res.Id.Is(-1);
            res.Data.Is("empty");
        }


        public IEnumerator ServerStreamingResult4()
        {
            yield return GetClient()
                .ServerStreamingResult4()
                .ResponseStream
                .ForEachAsync(x => { })
                .ToYieldInstruction();
        }

        public IEnumerator ServerStreamingResult5()
        {
            object res = null;
            yield return GetClient()
                .ServerStreamingResult5(new MyStructRequest(43, 643))
                .ResponseStream
                .ForEachAsync(x => res = x)
                .ToYieldInstruction();

            ((MyStructResponse)res).X.Is(43);
            ((MyStructResponse)res).Y.Is(643);
        }
    }
}