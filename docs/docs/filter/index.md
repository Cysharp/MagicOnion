# Filter fundamentals

TBW

:::info
This feature and document is applies to the server-side only. If you want to use client-side filter, please refer to [ClientFilter](client-filter).
:::

MagicOnion filter is powerful feature to hook before-after invoke. It is useful than gRPC server interceptor.

![image](https://user-images.githubusercontent.com/46207/50969421-cb465900-1521-11e9-8824-8a34cc52bbe4.png)

```csharp
// You can attach per class/method like [SampleFilter]
// for StreamingHub methods, implement StreamingHubFilterAttribute instead.
public class SampleFilterAttribute : MagicOnionFilterAttribute
{
    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        try
        {
            /* on before */
            await next(context); // next
            /* on after */
        }
        catch
        {
            /* on exception */
            throw;
        }
        finally
        {
            /* on finally */
        }
    }
}
```

Here is example of what kind of filter can be stacked.

![image](https://user-images.githubusercontent.com/46207/50969539-2bd59600-1522-11e9-84ab-15dd85e3dcac.png)

MagicOnion also provides an API for filters that is very similar to filter on ASP.NET Core MVC.
These APIs support flexible filter implementations.

- `IMagicOnionServiceFilter` interface
- `IStreamingHubFilter` interface
- `IMagicOnionFilterFactory<T>` interface
- `IMagicOnionOrderedFilter` interface

## Ordering
Filters can be ordered and they are executed in the following order:

```
[Ordered Filters] -> [Global Filters] -> [Class Filters] -> [Method Filters]
```

Unordered filters are treated as last (`int.MaxValue`) and executed in the order in which they are added.

## GlobalFilters
Filters that apply to the application globally can be added at `GlobalFilters` of `MagicOnionOptions`.

```csharp
services.AddMagicOnion(options =>
{
    options.GlobalFilters.Add<MyServiceFilter>();
    options.GlobalStreamingHubFilters.Add<MyHubFilter>();
});
```
