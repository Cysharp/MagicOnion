# フィルターの拡張性

:::info
この機能とドキュメントはサーバーサイドのみに適用されます。
:::

フィルターの実装は MagicOnionFilterAttribute や StreamingHubFilterAttribute を継承する以外にフィルター用のインターフェースを使用しての実装も可能です。フィルターのインターフェースは ASP.NET Core MVC のフィルター機構に似たプログラミングモデルを提供します。

フィルターの実装には以下のインターフェースを使用できます。

- `IMagicOnionFilterFactory<T>`: フィルターインターフェースを生成するファクトリー
- `IMagicOnionOrderedFilter`: フィルターの順序を指定するためのインターフェース
- `IMagicOnionServiceFilter`: Unary サービス用のフィルターインターフェース
- `IStreamingHubFilter`: StreamingHub サービス用のフィルターインターフェース


## フィルターインターフェースを使った実装
`IMagicOnionServiceFilter` と `IStreamingHubFilter` を実装することでフィルターを実装できます。`MagicOnionFilterAttribute` と `StreamingHubFilterAttribute` はこれらのインターフェースを簡単に使用するための実装を提供するものです。

例えばこの二つのインターフェースを実装することで Unary サービス、StreamingHub の両対応のフィルターを実装することが可能です。

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

## ファクトリーを使った実装

`IMagicOnionFilterFactory<T>` インターフェースはフィルターを生成するファクトリーメソッドを提供します。このファクトリーを使用するとフィルター属性の引数を使用しつつ、 DI を使用したフィルターインスタンスを柔軟に生成できます。

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
