# クライアントフィルター

クライアントフィルターはクライアント上で、サービスのメソッドの呼び出し前後をフックする強力な機能です。フィルターは gRPC サーバーインターセプターと似たような機能を提供しますが、HttpClient のハンドラーのような馴染みやすいプログラミングモデルを提供します。


:::info
現時点では Unary のみをサポートします。
:::

## 実装と使用方法
クライアントフィルターを実装するには `IClientFilter` インターフェースを実装します。これは HttpClient の HttpMessageHandler や ASP.NET Core のミドルウェアと同じプログラミングモデルです。

フィルター内では `next` デリゲートを呼び出すことで次のフィルターまたは実際のメソッドを呼び出します。例えば `next` の呼び出しをスキップしたり、`next` の呼び出しの例外をキャッチすることで例外時の処理を追加するといったことが可能です。

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

`ResponseContext` のインスタンスを新規に作成して返すことでメソッドを呼び出さずにフィルターで処理を終了することも可能です。これによりモックのような実装を実現できます。

:::warning
`RequestContext` から `CallOptions` を取得して変更することでリクエストヘッダーを変更することができます。ただし、`CallOptions` は MagicOnionClient インスタンスごとに保持されるため、リクエスト毎に追加すると重複登録となるため注意してください。
:::


実装したフィルターをクライアントで使用するには `MagicOnionClient.Create` の引数で `IClientFilter` の配列を指定します。

```csharp
var client = MagicOnionClient.Create<ICalcService>(channel, new IClientFilter[]
{
    new DemoFilter(),
    new LoggingFilter(),
    new AppendHeaderFilter(),
    new RetryFilter()
});
```

## サンプル実装例

以下はヘッダーを追加する例や、リクエストのログを出力する例、リトライ処理を行う例などのサンプルです。

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
