using RuntimeUnitTestToolkit;
using UnityEngine;
using System.Collections;
using ZeroFormatter;
using SharedLibrary;
using ZeroFormatter.Formatters;
using System;
using Grpc.Core.Logging;
using System.IO;
using System.Text;

namespace MagicOnion.Tests
{
    public static class UnitTestLoader
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            // Register First
            ZeroFormatterInitializer.Register();
            MagicOnionInitializer.Register();
            ZeroFormatter.Formatters.Formatter.RegisterArray<DefaultResolver, MyEnum>();

            // gRPC Config
            Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
            Grpc.Core.GrpcEnvironment.SetLogger(new MagicOnion.UnityDebugLogger());

            UnitTest.RegisterAllMethods<SimpleTest>();
            UnitTest.RegisterAllMethods<StandardTest>();
            UnitTest.RegisterAllMethods<ArgumentPatternTest>();
        }
    }
}