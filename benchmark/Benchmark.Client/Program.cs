using System;
using Grpc.Net.Client;
using Benchmark.Client;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

var hostAddress = "http://localhost:5000";
var itelation = 10000;
if (args.Length >= 1)
{
    hostAddress = args[0];
    if (args.Length >= 2)
    {
        itelation = int.Parse(args[1]);
    }
}

// Connect to the server using gRPC channel.
var channel = GrpcChannel.ForAddress(hostAddress);

await Task.Delay(TimeSpan.FromSeconds(3));
// Unary
Console.WriteLine($"Begin unary requests.");
var unary = new UnaryBenchmarkScenario(channel);
await unary.Run(itelation);
Console.WriteLine($"Completed all unary requests.");

// StreamingHub
Console.WriteLine($"Begin Streaming requests.");
await using var hub = new HubBenchmarkScenario(channel);
await hub.Run(itelation);
Console.WriteLine($"Completed Streaming requests.");

Console.ReadLine();