# 필터의 확장성

:::info
이 기능과 문서는 서버 측에만 적용됩니다.
:::

필터의 구현은 MagicOnionFilterAttribute나 StreamingHubFilterAttribute를 상속하는 것 외에도 필터용 인터페이스를 사용한 구현도 가능합니다. 필터의 인터페이스는 ASP.NET Core MVC의 필터 기구와 비슷한 프로그래밍 모델을 제공합니다.

필터의 구현에는 다음의 인터페이스를 사용할 수 있습니다.

- `IMagicOnionFilterFactory<T>`: 필터 인터페이스를 생성하는 팩토리
- `IMagicOnionOrderedFilter`: 필터의 순서를 지정하기 위한 인터페이스
- `IMagicOnionServiceFilter`: Unary 서비스용 필터 인터페이스
- `IStreamingHubFilter`: StreamingHub 서비스용 필터 인터페이스


## 필터 인터페이스를 사용한 구현
`IMagicOnionServiceFilter`와 `IStreamingHubFilter`를 구현함으로써 필터를 구현할 수 있습니다. `MagicOnionFilterAttribute`와 `StreamingHubFilterAttribute`는 이러한 인터페이스를 쉽게 사용하기 위한 구현을 제공하는 것입니다.

예를 들어 이 두 인터페이스를 구현함으로써 Unary 서비스, StreamingHub 모두에 대응하는 필터를 구현하는 것이 가능합니다.

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
class SampleFilterAttribute : IMagicOnionServiceFilter, IStreamingHubFilter, Attribute
{
    public async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        ...
    }

    public async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
    {
        ...
    }
}
```

## 팩토리를 사용한 구현

`IMagicOnionFilterFactory<T>` 인터페이스는 필터를 생성하는 팩토리 메소드를 제공합니다. 이 팩토리를 사용하면 필터 속성의 인자를 사용하면서, DI를 사용한 필터 인스턴스를 유연하게 생성할 수 있습니다.

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
class SampleFilterAttribute : Attribute, IMagicOnionFilterFactory<IMagicOnionServiceFilter>
{
    public string Name { get; set; }

    public SampleFilterAttribute(string name)
    {
        Name = name;
    }

    public IMagicOnionServiceFilter CreateInstance(IServiceProvider serviceProvider)
    {
        return new FilterImpl(serviceProvider.GetRequiredService<ILogger<SampleFilterAttribute>>());
    }

    class FilterImpl : IMagicOnionServiceFilter, IMagicOnionOrderedFilter
    {
        readonly string name;
        readonly ILogger logger;

        public int Order { get; set; } = int.MaxValue;

        public FilterImpl(string name, ILogger logger)
        {
            this.name = name;
            this.logger = logger;
        }

        public async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            logger.LogInformation($"SampleFilter[{name}] Begin: {context.Path}");
            await next(context);
            logger.LogInformation($"SampleFilter[{name}] End: {context.Path}");
        }
    }
}

[SampleFilter("MyService")]
class MyService : ServiceBase<IMyService>
{
    ...
}
```
