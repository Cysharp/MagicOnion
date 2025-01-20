# Dependency Injection (M.E.DI)
ASP.NET Core Web APIs や ASP.NET Core MVC と同様に、MagicOnion ではサービスや StreamingHub に対するコンストラクターインジェクションをサポートしています。

```csharp
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    IOptions<MyConfig> config;
    ILogger<MyFirstService> logger;

    public MyFirstService(IOptions<MyConfig> config, ILogger<MyFirstService> logger)
    {
        this.config = config;
        this.logger = logger;
    }

    // ...
}
```

Unary サービス、 StreamingHub のどちらのコンストラクターであってもコンストラクターインジェクションに対応しています。しかし StreamingHub は Unary と異なりクライアントが接続されている間は Transient なインスタンスが維持される点に注意してください。これはデータベースアクセスのようなサービスを DI から取得した場合に想定外のライフタイムを持つことで問題になるといったことが考えられます。
