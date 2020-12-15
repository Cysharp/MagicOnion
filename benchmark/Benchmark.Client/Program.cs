using System;
using Grpc.Net.Client;
using Benchmark.Client;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Benchmark.Client.Reports;

var hostAddress = "http://localhost:5000";
var itelation = 10000;
var id = Guid.NewGuid().ToString();
if (args.Length >= 1)
{
    hostAddress = args[0];
    if (args.Length >= 2)
    {
        itelation = int.Parse(args[1]);
        if (args.Length >= 3)
        {
            id = args[2];
        }
    }
}

// Connect to the server using gRPC channel.
var channel = GrpcChannel.ForAddress(hostAddress);
var reporter = new BenchReporter(id);

await Task.Delay(TimeSpan.FromSeconds(3));
// Unary
Console.WriteLine($"Begin unary requests.");
var unary = new UnaryBenchmarkScenario(channel, reporter);
await unary.Run(itelation);
Console.WriteLine($"Completed all unary requests.");

// StreamingHub
Console.WriteLine($"Begin Streaming requests.");
await using var hub = new HubBenchmarkScenario(channel, reporter);
await hub.Run(itelation);
Console.WriteLine($"Completed Streaming requests.");

var benchJson = reporter.OutputJson();
Console.WriteLine(benchJson);
Console.ReadLine();