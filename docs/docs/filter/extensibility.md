# Filter extensibility

:::info
This feature and document is applies to the server-side only.
:::

Implementing filters by inheriting from `MagicOnionFilterAttribute` or `StreamingHubFilterAttribute` is not the only way to implement filters. You can also implement filters using filter interfaces. These interfaces provide a programming model similar to ASP.NET Core MVC filters.

You can use the following interfaces to implement filters:

- `IMagicOnionFilterFactory<T>`: Factory interface to generate filter instances
- `IMagicOnionOrderedFilter`: Interface to specify the order of filters
- `IMagicOnionServiceFilter`: Interface for Unary service filters
- `IStreamingHubFilter`: Interface for StreamingHub filters

## Implementing filter using filter interfaces
You can implement filters by implementing the `IMagicOnionServiceFilter` and `IStreamingHubFilter` interfaces. `MagicOnionFilterAttribute` and `StreamingHubFilterAttribute` provide implementations to make it easier to use these interfaces.

For example, by implementing these two interfaces, you can implement filters that support both Unary services and StreamingHub.

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

## Implementing filter using filter factory

`IMagicOnionFilterFactory<T>` interface provides a factory method to generate filters. By using this factory, you can flexibly generate filter instances using DI while using the arguments of the filter attribute.

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
