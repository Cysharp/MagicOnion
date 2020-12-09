using System;
using Grpc.Net.Client;
using Benchmark.Client;
using System.Diagnostics;

// Connect to the server using gRPC channel.
var channel = GrpcChannel.ForAddress("https://localhost:5001");

// Unary
Console.WriteLine($"Begin unary requests.");
var unary = new UnaryBenchmarkScenario(channel);
await unary.Run(10000);
Console.WriteLine($"Completed all unary requests.");

// StreamingHub
Console.WriteLine($"Begin Streaming requests.");
await using var hub = new HubBenchmarkScenario(channel);
await hub.Run(10000);
Console.WriteLine($"Completed Streaming requests.");

Console.ReadLine();
