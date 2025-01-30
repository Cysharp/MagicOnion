# Client Filter

Client Filter is a powerful feature to hook before-after service method invocation. Filter like gRPC client interceptor but more familiar programming model like HttpClient handlers or ASP.NET Core middlewares.

:::info
Currently, the feature is only supported for Unary.
:::

## Implementation and Usage
To implement a filter, inherit from `IClientFilter` and implement the `SendAsync` method. This is the same programming model as HttpClient's `HttpMessageHandler` or ASP.NET Core middleware.

In the filter, you call the `next` delegate to call the next filter or the actual method. You can skip calling `next` or catch exceptions from calling `next` to add exception handling.

```csharp
public class DemoFilter : IClientFilter
{
    public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
    {
        try
        {
            // Before Request: context.MethodPath/CallOptions/Items
            // Console.WriteLine("Request Begin:" + context.MethodPath);
            // ...

            var response = await next(context); /* Call next filter or method body */

            // After Request: response.GetStatus/GetTrailers/GetResponseAs<T>
            // var result = await response.GetResponseAs<T>();
            // var status = response.GetStatus();
            // ...

            return response;
        }
        catch (RpcException ex)
        {
            /* gRPC Exception */
            throw;
        }
        catch (Exception ex)
        {
            /* Other Exception */
            throw;
        }
        finally
        {
            /* Clean-up */
        }
    }
}
```

You can end the processing in the filter without calling the method by creating a new instance of `ResponseContext`. This allows you to implement a mock-like implementation.

:::warning
You can change the request header by getting and modifying `CallOptions` from `RequestContext`. However, `CallOptions` is holded per MagicOnionClient instance, so be careful not to add duplicate headers for each request.
:::


To use the implemented filter in the client, specify an array of `IClientFilter` in the arguments of `MagicOnionClient.Create`.

```csharp
var client = MagicOnionClient.Create<ICalcService>(channel, new IClientFilter[]
{
    new DemoFilter(),
    new LoggingFilter(),
    new AppendHeaderFilter(),
    new RetryFilter()
});
```

## Sample Implementation
The following are examples of adding headers, outputting request logs, and retrying.

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
