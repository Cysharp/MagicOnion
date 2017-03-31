using RuntimeUnitTestToolkit;
using UnityEngine;
using System.Collections;
using SharedLibrary;
using MessagePack;
using System;
using Grpc.Core.Logging;
using System.IO;
using System.Text;
using Grpc.Core;
using MagicOnion.Client;

namespace MagicOnion.Tests
{
    public static class UnitTestLoader
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            // Register First
            //ZeroFormatterInitializer.Register();
            MagicOnionInitializer.Register();
            //ZeroFormatter.Formatters.Formatter.RegisterArray<DefaultResolver, MyEnum>();

            // Button ON
            RuntimeUnitTestToolkit.UnitTest.AddCustomAction("Use Local", () =>
            {
                UnitTestClient.endPoint = "local";
            });
            RuntimeUnitTestToolkit.UnitTest.AddCustomAction("Use Remote", () =>
            {
                UnitTestClient.endPoint = "104.199.192.165";
            });

#if UNITY_EDITOR_WIN
            // Debugger Attached, avoid UnityEditor crash
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Grpc.Core.GrpcEnvironment.IsDebugging = true;
            }
#endif

            // gRPC Config
            // Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            // Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
            Grpc.Core.GrpcEnvironment.SetLogger(new MagicOnion.UnityDebugLogger());

            // Register Tests
            UnitTest.RegisterAllMethods<SimpleTest>();
            UnitTest.RegisterAllMethods<StandardTest>();
            UnitTest.RegisterAllMethods<ArgumentPatternTest>();
            UnitTest.RegisterAllMethods<HeartbeatTest>();
            UnitTest.RegisterAllMethods<MetadataTest>();
        }
    }

    public static class UnitTestClient
    {
        internal static string endPoint = "localhost";

        public static T Create<T>() where T : IService<T>
        {
            var channel = new Channel(endPoint, 12345, ChannelCredentials.Insecure);

            var client = MagicOnionClient.Create<T>(channel).WithDeadline(DateTime.UtcNow.AddSeconds(10));

            return client;
        }

        public static Channel GetChannel()
        {
            var channel = new Channel(endPoint, 12345, ChannelCredentials.Insecure);
            return channel;
        }
    }
}