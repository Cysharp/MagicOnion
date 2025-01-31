# フィルターの基礎

:::info
この機能とドキュメントはサーバーサイドのみに適用されます。クライアントサイドのフィルターを使用する場合は、[クライアントフィルター](client-filter)を参照してください。
:::

MagicOnion はサービスのメソッドの呼び出し前後にフックするフィルターという強力な機能を提供します。フィルターは gRPC サーバーインターセプターと似たような機能を提供しますが、HttpClient のハンドラーや ASP.NET Core のミドルウェアのような馴染みやすいプログラミングモデルを提供します。

![image](https://user-images.githubusercontent.com/46207/50969421-cb465900-1521-11e9-8824-8a34cc52bbe4.png)

下記の図はフィルターが複数設定され、処理される構成のイメージです。

![image](https://user-images.githubusercontent.com/46207/50969539-2bd59600-1522-11e9-84ab-15dd85e3dcac.png)

## 実装と使用方法

フィルターの実装は `MagicOnionFilterAttribute` を継承して `Invoke` メソッドを実装します。

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

フィルター内では `next` デリゲートを呼び出すことで次のフィルターまたは実際のメソッドを呼び出します。例えば `next` の呼び出しをスキップしたり、`next` の呼び出しの例外をキャッチすることで例外時の処理を追加するといったことが可能です。

実装したフィルターを適用するには Unary サービスのクラスまたはメソッドに `[SampleFilter]` のように属性を付与します。グローバルフィルターとしてアプリケーション全体に適用も可能です。

MagicOnion は ASP.NET Core MVC によく似たフィルターの API も提供しています。
これらの API は柔軟なフィルターの実装をサポートします。詳細については [フィルターの拡張性](extensibility) を参照してください。


## グローバルフィルター
フィルターは MagicOnionOptions の `GlobalFilters` に追加することでアプリケーション全体に適用できます。

```csharp
services.AddMagicOnion(options =>
{
    options.GlobalFilters.Add<MyServiceFilter>();
    options.GlobalStreamingHubFilters.Add<MyHubFilter>();
});
```

## 処理の順番
フィルターは順番を指定でき、以下の順番で実行されます。

```
[順番の指定されたフィルター] -> [グローバルフィルター] -> [クラスフィルター] -> [メソッドフィルター]
```

順番の指定されていないフィルターは最後 (`int.MaxValue`) として扱われ、追加された順番で実行されます。

## ASP.NET Core のミドルウェアとの統合
MagicOnion サーバーでは ASP.NET Core のミドルウェアも使用できますがフィルターよりも先に実行されます。これは gRPC の呼び出しが純粋な HTTP リクエストとして処理されるのに対して、フィルターは gRPC の呼び出しとして処理が始まった後に実行されるためです。

<img src={require('/img/docs/fig-filter-with-middleware.png').default} alt="" style={{height: '320px'}} />
