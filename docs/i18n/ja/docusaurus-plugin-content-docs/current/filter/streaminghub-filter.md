# StreamingHub フィルター

:::info
この機能とドキュメントはサーバーサイドのみに適用されます。
:::

StreamingHub にもフィルターを適用できますがフィルターの種類と挙動に注意が必要です。このページでは StreamingHub でのフィルターの基本的な使い方と注意点について説明します。

## Unary サービス向けのフィルターを使う
Unary サービス向けに実装したフィルター MagicOnionFilter を StreamingHub に適用できます。ただし StreamingHub に適用した場合にフィルターが実行されるのは接続時 (`Connect`) のみです。接続後の **Hub メソッド呼び出し時にはフィルターは実行されません**。そのため適用できるのはクラスまたはグローバルのみです。

これは StreamingHub の接続状況メトリクスや認証、といったものを取り扱う場合には適していますが、Hub メソッドごとにフックしたいといった場合には次に説明する StreamingHub フィルターを使用してください。

:::tip
Unary 向けのフィルターをグローバルフィルターに設定した場合、StreamingHub にも適用されることに注意が必要です。例えばメソッドの実行時間の計測などを行う場合 StreamingHub によって意図せず Connect が記録される、といったことが起こりえます。
:::

## StreamingHub フィルターを使う
StreamingHub フィルターは Hub メソッドの呼び出し前後にフックするフィルターです。Unary サービスのフィルターとほぼ同じですが MagicOnionFilterAttribute の代わりに StreamingHubFilterAttribute を継承して `Invoke` メソッドを実装します。

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

StreamingHub フィルターは Hub メソッドの呼び出し単位で実行されるため、エラーハンドリングやロギング、メソッドの計測などに適しています。

StreamingHub フィルターも通常のフィルターと同様に拡張用のインターフェースが用意されているため、柔軟なフィルター実装が可能です。詳しくは [フィルターの拡張性](/filter/extensibility) を参照してください。
