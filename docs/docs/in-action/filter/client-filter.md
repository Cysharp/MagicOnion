# Client Filter
MagicOnion client-filter is a powerful feature to hook before-after invoke. It is useful than gRPC client interceptor.

:::info
Currently, the feature is only supported for Unary.
:::

```csharp
// you can attach in MagicOnionClient.Create.
var client = MagicOnionClient.Create<ICalcService>(channel, new IClientFilter[]
{
    new LoggingFilter(),
    new AppendHeaderFilter(),
    new RetryFilter()
});
```

You can create custom client-filter by implements `IClientFilter.SendAsync`.

```csharp
public class IDemoFilter : IClientFilter
{
    public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
    {
        try
        {
            /* Before Request, context.MethodPath/CallOptions/Items, etc */

            var response = await next(context); /* Call next filter or method body */

            /* After Request, response.GetStatus/GetTrailers/GetResponseAs<T>, etc */

            return response;
        }
        catch (RpcException ex)
        {
            /* Get gRPC Error Response */
            throw;
        }
        catch (OperationCanceledException ex)
        {
            /* If canceled */
            throw;
        }
        catch (Exception ex)
        {
            /* Other Exception */
            throw;
        }
        finally
        {
            /* Common Finalize */
        }
    }
}
```

Here is the sample filters, you can imagine what you can do.

```csharp
public class AppendHeaderFilter : IClientFilter
{
    public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
    {
        // add the common header(like authentication).
        var header = context.CallOptions.Headers;
        if (!header.Any(x => x.Key == "x-foo"))
        {
            header.Add("x-foo", "abcdefg");
            header.Add("x-bar", "hijklmn");
        }

        return await next(context);
    }
}

public class LoggingFilter : IClientFilter
{
    public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
    {
        Console.WriteLine("Request Begin:" + context.MethodPath); // Debug.Log in Unity.

        var sw = Stopwatch.StartNew();
        var response = await next(context);
        sw.Stop();

        Console.WriteLine("Request Completed:" + context.MethodPath + ", Elapsed:" + sw.Elapsed.TotalMilliseconds + "ms");

        return response;
    }
}

public class ResponseHandlingFilter : IClientFilter
{
    public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
    {
        var response = await next(context);

        if (context.MethodPath == "ICalc/Sum")
        {
            // You can cast response type.
            var sumResult = await response.GetResponseAs<int>();
            Console.WriteLine("Called Sum, Result:" + sumResult);
        }

        return response;
    }
}

public class MockRequestFilter : IClientFilter
{
    public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
    {
        if (context.MethodPath == "ICalc/Sum")
        {
            // don't call next, return mock result.
            return new ResponseContext<int>(9999);
        }

        return await next(context);
    }
}

public class RetryFilter : IClientFilter
{
    public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
    {
        Exception lastException = null;
        var retryCount = 0;
        while (retryCount != 3)
        {
            try
            {
                // using same CallOptions so be careful to add duplicate headers or etc.
                return await next(context);
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
            retryCount++;
        }

        throw new Exception("Retry failed", lastException);
    }
}
```
