# StreamingHub Filter

:::info
This feature and document is applies to the server-side only.
:::

StreamingHub also supports filters. However, the type and behavior of the filters need to be considered. This page explains the basic usage and notes of filters in StreamingHub.

## Use Unary service filters
You can apply the filter MagicOnionFilter implemented for Unary services to StreamingHub by applying it to StreamingHub. However, when applied to StreamingHub, the filter is executed only at the time of connection (`Connect`). **The filter is not executed when calling Hub methods after connection**. Therefore, it can be applied only to classes or globally.

This is suitable for handling connection status metrics, authentication, etc., but if you want to hook for each Hub method, use StreamingHub filters described next.

:::tip
If you set the filter for Unary services to a global filter, it will also be applied to StreamingHub. Be careful that unintended Connect may be recorded by StreamingHub when measuring the execution time of methods, etc.
:::

## Use StreamingHub filters
StreamingHub filters are filters that hook before and after the call to the Hub method. It is similar to Unary service filters, but instead of inheriting from `MagicOnionFilterAttribute`, inherit from `StreamingHubFilterAttribute` and implement the `Invoke` method.

```csharp
class StreamingHubFilterAttribute : StreamingHubFilterAttribute
{
    public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
    {
        // before invoke
        try
        {
            await next(context);
        }
        finally
        {
            // after invoke
        }
    }
}
```

StreamingHub filters are executed on a per-Hub method call basis, making them suitable for error handling, logging, method measurement, etc.

StreamingHub filters also provide an extension interface like normal filters, allowing for flexible filter implementations. For more information, see [Filter Extensibility](extensibility).
