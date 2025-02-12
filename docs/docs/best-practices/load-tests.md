# Load Tests

This page describes how to perform load tests on MagicOnion applications.

MagicOnion uses a custom protocol based on gRPC, so it cannot be load tested using common load testing tools such as [JMeter](https://jmeter.apache.org/) or [k6](https://k6.io/).

This may seem like a disadvantage, but MagicOnion clients are written in C# (.NET), making it easy to create and execute custom scenarios for load testing using .NET applications.

MagicOnion applications often execute scenarios with multiple steps using real-time communication, and when creating such complex scenarios, you can maximize the reuse of code and the implementation knowledge of the client.

## Choose a Load Testing Framework

Since the workers and clients for load testing must be MagicOnion clients, they must be .NET applications. The following are some load testing frameworks that meet this requirement:

- [DFrame](https://github.com/Cysharp/DFrame)
- [NBomber](https://nbomber.com/)

You can also implement your own .NET applications, such as console applications or Unity headless clients, without using a framework.

### Load Testing with DFrame

[DFrame](https://github.com/Cysharp/DFrame) is an open-source distributed .NET-based load testing framework provided by Cysharp. DFrame allows you to write load test scenarios in Workloads and apply load using multiple workers.

The following is an example of a load test using DFrame. For more information, refer to the DFrame documentation.

```csharp
using DFrame;
using MagicOnion;

DFrameApp.Run(7312, 7313); // WebUI:7312, WorkerListen:7313

public class SampleWorkload : Workload
{
    public override async Task ExecuteAsync(WorkloadContext context)
    {
        using var channel = GrpcChannel.ForAddress("https://api.example.com");

        var apiClient = MagicOnionClient.Create<IAccountService>(channel);
        var accessToken = await apiClient.CreateUserAsync($"User-{Guid.CreateVersion7()}", "p@ssword1");

        var hubClient = await StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(
            channel,
            new GreeterHubReceiver(),
            StreamingHubClientOptions.CreateWithDefault(callOptions: new CallOptions(){
                Headers = new Metadata
                {
                    { "Authorization", $"Bearer {accessToken}" },
                }
            })
        );
        var result = await hubClient.HelloAsync();
    }
}
```

:::note
DFrame uses MagicOnion for communication between the DFrame controller and workers, so be careful of version conflicts with MagicOnion.Client.
:::

### Load Testing with NBomber

[NBomber](https://nbomber.com/) is a .NET-based distributed load testing framework commercially provided by NBomber LLC.

The following is an example of a load test scenario using NBomber. For more information, refer to the NBomber documentation.

```csharp
var scenario = Scenario.Create("MagicOnionTest", async ctx =>
{
    using var channel = GrpcChannel.ForAddress("https://api.example.com");

    var createUser = await Step.Run("create", ctx, async () =>
    {
        var apiClient = MagicOnionClient.Create<IAccountService>(channel);
        var accessToken = await apiClient.CreateUserAsync($"User-{Guid.CreateVersion7()}", "p@ssword1");
        return Response.Ok();
    });

    IGreeterHub? hubClient = null;
    var receiver = new GreeterHubReceiver();
    var connect = await Step.Run("connect", ctx, async () =>
    {

        hubClient = await StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(
            channel,
            new GreeterHubReceiver(),
            StreamingHubClientOptions.CreateWithDefault(callOptions: new CallOptions(){
                Headers = new Metadata
                {
                    { "Authorization", $"Bearer {accessToken}" },
                }
            })
        );
        return Response.Ok();
    });

    var hello = await Step.Run("hello", ctx, async () =>
    {
        var result = await hubClient.HelloAsync();
        return Response.Ok();
    });

    return Response.Ok();
});
```
