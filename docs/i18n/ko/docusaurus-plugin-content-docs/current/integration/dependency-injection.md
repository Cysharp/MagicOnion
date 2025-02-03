# Dependency Injection (M.E.DI)
ASP.NET Core Web APIs나 ASP.NET Core MVC와 마찬가지로, MagicOnion에서는 서비스와 StreamingHub에 대한 생성자 주입을 지원합니다.

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

Unary 서비스, StreamingHub 둘 다의 생성자에서 생성자 주입을 지원합니다. 하지만 StreamingHub는 Unary와 달리 클라이언트가 연결되어 있는 동안은 Transient한 인스턴스가 유지된다는 점에 주의해 주세요. 이는 데이터베이스 액세스와 같은 서비스를 DI에서 가져온 경우에 예상치 못한 수명을 가짐으로써 문제가 될 수 있습니다.
