# Dependency Injection (M.E.DI)
MagicONion supports constructor injection for services and StreamingHubs, similar to ASP.NET Core Web APIs and ASP.NET Core MVC.

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

Both the Unary service and StreamingHub constructor support constructor injection. However, StreamingHub is different from Unary in that it maintains a transient instance while the client is connected. This may cause problems if you get a service like database access from DI and it has an unexpected lifetime.
