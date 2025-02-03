# 클라이언트 필터

클라이언트 필터는 클라이언트 상에서, 서비스의 메소드 호출 전후를 후크하는 강력한 기능입니다. 필터는 gRPC 서버 인터셉터와 비슷한 기능을 제공하지만, HttpClient의 핸들러와 같은 친숙한 프로그래밍 모델을 제공합니다.

:::info
현시점에서는 Unary만을 지원합니다.
:::

## 구현과 사용 방법
클라이언트 필터를 구현하려면 `IClientFilter` 인터페이스를 구현합니다. 이는 HttpClient의 HttpMessageHandler나 ASP.NET Core의 미들웨어와 같은 프로그래밍 모델입니다.

필터 내에서는 `next` 델리게이트를 호출함으로써 다음 필터 또는 실제 메소드를 호출합니다. 예를 들어 `next`의 호출을 스킵하거나, `next`의 호출의 예외를 캐치함으로써 예외 시의 처리를 추가하는 등의 것이 가능합니다.

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

`ResponseContext` 인스턴스를 새로 생성하여 반환함으로써 메소드를 호출하지 않고 필터에서 처리를 종료하는 것도 가능합니다. 이를 통해 목(mock)과 같은 구현을 실현할 수 있습니다.

:::warning
`RequestContext`에서 `CallOptions`를 취득하여 변경함으로써 요청 헤더를 변경할 수 있습니다. 단, `CallOptions`는 MagicOnionClient 인스턴스마다 보유되기 때문에, 요청마다 추가하면 중복 등록이 되므로 주의해 주세요.
:::

구현한 필터를 클라이언트에서 사용하려면 `MagicOnionClient.Create`의 인자로 `IClientFilter`의 배열을 지정합니다.

```csharp
var client = MagicOnionClient.Create<ICalcService>(channel, new IClientFilter[]
{
    new DemoFilter(),
    new LoggingFilter(),
    new AppendHeaderFilter(),
    new RetryFilter()
});
```

## 샘플 구현 예

다음은 헤더를 추가하는 예시나, 요청의 로그를 출력하는 예시, 재시도 처리를 수행하는 예시 등의 샘플입니다.

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
