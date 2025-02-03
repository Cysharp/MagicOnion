# Dependency Injection

:::info
이 기능과 문서는 서버 측에만 적용됩니다.
:::

MagicOnion 필터는 Dependency Injection을 지원합니다. `FromTypeFilter`, `FromServiceFilter`를 사용하거나 `IMagicOnionFilterFactory`를 사용하는 두 가지 방법으로 필터를 활성화할 수 있습니다.

다음은 `FromTypeFilter`, `FromServiceFilter`를 사용하는 예시입니다.

```csharp
public class MyServiceFilterAttribute : MagicOnionFilterAttribute
{
    private readonly ILogger _logger;

    // the `logger` parameter will be injected at instantiating.
    public MyServiceFilterAttribute(ILogger<MyServiceFilterAttribute> logger)
    {
        _logger = logger;
    }

    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        _logger.LogInformation($"MyServiceFilter Begin: {context.Path}");
        await next(context);
        _logger.LogInformation($"MyServiceFilter End: {context.Path}");
    }
}
```

생성자 주입을 사용하여 속성으로 필터를 등록합니다(`[FromTypeFilter]`와 `[FromServiceFilter]`를 사용할 수 있습니다).

```csharp
[FromTypeFilter(typeof(MyFilterAttribute))]
public class MyService : ServiceBase<IMyService>, IMyService
{
    // The filter will instantiate from type.
    [FromTypeFilter(typeof(MySecondFilterAttribute))]
    public UnaryResult<int> Foo()
    {
        return UnaryResult(0);
    }

    // The filter will instantiate from type with some arguments. if the arguments are missing, it will be obtained from `IServiceProvider`
    [FromTypeFilter(typeof(MyThirdFilterAttribute), Arguments = new object[] { "foo", 987654 })]
    public UnaryResult<int> Bar()
    {
        return UnaryResult(0);
    }

    // The filter instance will be provided via `IServiceProvider`.
    [FromServiceFilter(typeof(MyFourthFilterAttribute))]
    public UnaryResult<int> Baz()
    {
        return UnaryResult(0);
    }
}
```

다음은 `IMagicOnionFilterFactory<T>`를 사용하는 예시입니다.

이는 속성에 대한 매개변수를 가지면서도 DI를 사용할 때 깔끔한 작성 방법입니다.

```csharp
public class MyServiceFilterAttribute : Attribute, IMagicOnionFilterFactory<IMagicOnionServiceFilter>, IMagicOnionOrderedFilter
{
    readonly string label;

    public int Order { get; set; } = int.MaxValue;

    public MyServiceFilterAttribute(string label)
    {
        this.label = label;
    }

    public IMagicOnionServiceFilter CreateInstance(IServiceProvider serviceProvider)
        => new MyServiceFilter(serviceProvider.GetRequiredService<ILogger<MyServiceFilterAttribute>>());

    class MyServiceFilter : IMagicOnionServiceFilter
    {
        readonly string label;
        readonly ILogger logger;

        public MyServiceFilter(string label, ILogger<MyServiceFilterAttribute> logger)
        {
            this.label = label;
            this.logger = logger;
        }

        public async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            logger.LogInformation($"[{label}] MyServiceFilter Begin: {context.Path}");
            await next(context);
            logger.LogInformation($"[{label}] MyServiceFilter End: {context.Path}");
        }
    }
}
```
```csharp
[MyServiceFilter("Class")]
public class MyService : ServiceBase<IMyService>, IMyService
{
    [MyServiceFilter("Method")]
    public UnaryResult<int> Foo()
    {
        return UnaryResult(0);
    }
}
```
