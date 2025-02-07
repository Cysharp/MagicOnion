# 負荷テスト

このページでは MagicOnion アプリケーションの負荷テストについて説明します。

MagicOnion は gRPC をベースにした独自のプロトコルを使用しているため、一般的な負荷テストツールである [JMeter](https://jmeter.apache.org/) や [k6](https://k6.io/) などのツールを使用して負荷テストを行えません。

これはデメリットのように見えるかもしれませんが、MagicOnion のクライアントは C# (.NET) であり、負荷テスト用の .NET アプリケーションでカスタムシナリオを作成、実行するのが容易というメリットがあります。

特に MagicOnion アプリケーションはリアルタイム通信を使用したステートフルで複数のステップのシナリオを実行することが多く、このような複雑なシナリオを作成する際にコードの再利用を含め、クライアントの実装知識を最大限活用できます。

## 負荷テストフレームワークを選択する

負荷テストのワーカーやクライアントは MagicOnion クライアントである必要があるため必然的に .NET アプリケーションであることが求められます。これを満たす負荷テストを行うフレームワークとしては以下のようなものがあります。

- [DFrame](https://github.com/Cysharp/DFrame)
- [NBomber](https://nbomber.com/)

またフレームワークを使用せず、コンソールアプリケーションや Unity ヘッドレスクライアントなど独自の .NET アプリケーションを実装することも可能です。

### DFrame を使用した負荷テスト

[DFrame](https://github.com/Cysharp/DFrame) は Cysharp の提供しているオープンソースの分散型の .NET ベースの負荷テストフレームワークです。DFrame では Workload という単位で負荷テストシナリオを記述し、複数のワーカーを使用して負荷をかけることが可能です。

以下は DFrame を使用した負荷テストの記述例です。詳しくは DFrame のドキュメントを参照してください。

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
DFrame は DFrame のコントローラーとワーカーの通信に MagicOnion を使用しているため、MagicOnion.Client のバージョンの競合に注意してください。
:::

### NBomber を使用した負荷テスト

[NBomber](https://nbomber.com/) は NBomber LLC の提供している有償の .NET ベースの分散負荷テストフレームワークです。

以下は NBomber での負荷テストシナリオの記述例です。詳しくは NBomber のドキュメントを参照してください。

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
