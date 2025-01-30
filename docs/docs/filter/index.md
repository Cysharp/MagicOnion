# Filter fundamentals

:::info
This feature and document is applies to the server-side only. If you want to use client-side filter, see [Client Filter](client-filter).
:::

MagicOnion provides a powerful feature called filters that allows you to hook into the service method invocation before and after. Filters provide a similar functionality to gRPC server interceptors, but with a more familiar programming model like HttpClient handlers or ASP.NET Core middlewares.

![image](https://user-images.githubusercontent.com/46207/50969421-cb465900-1521-11e9-8824-8a34cc52bbe4.png)

The following diagram illustrates the configuration of multiple filters being set and processed.

![image](https://user-images.githubusercontent.com/46207/50969539-2bd59600-1522-11e9-84ab-15dd85e3dcac.png)

## Implementation and Usage

To implement a filter, inherit from `MagicOnionFilterAttribute` and implement the `Invoke` method.

```csharp
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

In a filter, you call the `next` delegate to call the next filter or the actual method. You can skip calling `next` or catch exceptions from calling `next` to add exception handling.

To apply the implemented filter, add an attribute to the class or method of the Unary service like `[SampleFilter]`. You can also apply it globally to the entire application.

MagicOnion also provides an API for filters that is very similar to filters in ASP.NET Core MVC. These APIs support flexible filter implementations. For more information, see [Filter Extensibility](extensibility).

## Global Filters
You can apply filters to the entire application by adding them to `GlobalFilters` in `MagicOnionOptions`.

```csharp
services.AddMagicOnion(options =>
{
    options.GlobalFilters.Add<MyServiceFilter>();
    options.GlobalStreamingHubFilters.Add<MyHubFilter>();
});
```

## Processing Order
Filters can be ordered and they are executed in the following order:

```
[Ordered filters] -> [Global filters] -> [Class filters] -> [Method filters]
```

Not ordered filters are treated as last (`int.MaxValue`) and executed in the order in which they are added.

## Integration with ASP.NET Core Middleware
In a MagicOnion server, you can also use ASP.NET Core middlewares. Middlewares are executed before filters because gRPC calls are processed as pure HTTP requests, while filters are executed after gRPC calls have started processing.

<img src={require('/img/docs/fig-filter-with-middleware.png').default} alt="" style={{height: '320px'}} />
