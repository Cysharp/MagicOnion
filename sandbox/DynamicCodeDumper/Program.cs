using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicCodeDumper
{
    public interface IMessageReceiver
    {
        Task OnReceiveMessage(int senderId, string message);
    }
    public interface IChatHub : IStreamingHub<IChatHub, IMessageReceiver>
    {
        Task EchoAsync(string message);
        Task<string> EchoRetrunAsync(string message);
    }

    class Program
    {
        static void Main(string[] args)
        {
            //var _ = DynamicBroadcasterBuilder<IMessageReceiver>.BroadcasterType;
            //var builder = MagicOnion.Server.Hubs.AssemblyHolder.Save();
            var _ = StreamingHubClientBuilder<IChatHub, IMessageReceiver>.ClientType;
            var builder = MagicOnion.Client.StreamingHubClientAssemblyHolder.Save();
            Verify(builder);
        }

        static void Verify(params AssemblyBuilder[] builders)
        {
            var path = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\PEVerify.exe";

            foreach (var targetDll in builders)
            {
                var psi = new ProcessStartInfo(path, targetDll.GetName().Name + ".dll")
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                var p = Process.Start(psi);
                var data = p.StandardOutput.ReadToEnd();
                Console.WriteLine(data);
            }
        }
    }
}
