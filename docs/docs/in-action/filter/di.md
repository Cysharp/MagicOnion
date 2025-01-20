# Dependency Injections

:::info
This feature and document is applies to the server-side only.
:::

MagicOnion filters supports [Dependency Injection](#dependency-injection). There are two ways to activate a filter by using `FromTypeFilter`, `FromServiceFitler` or by using `IMagicOnionFilterFactory`.

The following is an example of how to use `FromTypeFilter`, `FromServiceFitler`.

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

Register filters using attributes with constructor injection(you can use `[FromTypeFilter]` and `[FromServiceFilter]`).

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

The following is an example of how to use `IMagicOnionFilterFactory<T>`.

This is a clean way of writing when using DI while still having parameters for the attributes.

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
