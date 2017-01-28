#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;
using System.Diagnostics;

namespace Sandbox.ConsoleServer.Services
{
    public class SendMetadata : ServiceBase<ISendMetadata>, ISendMetadata
    {
        public async Task<UnaryResult<int>> PangPong()
        {
            var s1 = new StackTrace().ToString();
            s1 = s1 + s1 + s1 + s1 + s1;

            var lineSplit = s1.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            for (int i = 0; i < lineSplit.Length; i++)
            {
                if (!lineSplit[i].Contains("System.Runtime.CompilerServices.TaskAwaiter"))
                {
                    sb.Append(lineSplit[i]);
                }
                if (sb.Length >= 10000)
                {
                    sb.Append("----Omit Message(message size is too long)----");
                    break;
                }
            }
            var str = sb.ToString();

            var bts = Encoding.UTF8.GetBytes(str);
            Console.WriteLine("bytescount:" + bts.Length);

            this.Context.CallContext.ResponseTrailers.Add("test_data-bin", bts);

            return UnaryResult(10);
        }
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously