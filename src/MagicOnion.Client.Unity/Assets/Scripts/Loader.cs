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
    public static class UnitySystemConsoleRedirector
    {
        private class UnityTextWriter : TextWriter
        {
            private StringBuilder buffer = new StringBuilder();

            public override void Flush()
            {
                Debug.Log(buffer.ToString());
                buffer.Length = 0;
            }

            public override void Write(string value)
            {
                buffer.Append(value);
                if (value != null)
                {
                    var len = value.Length;
                    if (len > 0)
                    {
                        var lastChar = value[len - 1];
                        if (lastChar == '\n')
                        {
                            Flush();
                        }
                    }
                }
            }

            public override void Write(char value)
            {
                buffer.Append(value);
                if (value == '\n')
                {
                    Flush();
                }
            }

            public override void Write(char[] value, int index, int count)
            {
                Write(new string(value, index, count));
            }

            public override Encoding Encoding
            {
                get { return Encoding.Default; }
            }
        }

        public static void Redirect()
        {
            Console.SetOut(new UnityTextWriter());
            Console.SetError(new UnityTextWriter());
        }
    }


    public static class UnitTestLoader
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            UnitySystemConsoleRedirector.Redirect();

            // Register First
            ZeroFormatterInitializer.Register();
            MagicOnionInitializer.Register();
            ZeroFormatter.Formatters.Formatter.RegisterArray<DefaultResolver, MyEnum>();

            // gRPC Config
            Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
            Grpc.Core.GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            UnitTest.RegisterAllMethods<SimpleTest>();
            UnitTest.RegisterAllMethods<StandardTest>();
            UnitTest.RegisterAllMethods<ArgumentPatternTest>();
        }
    }
}