# 生バイト配列のレスポンス
フィルター内で `ServiceContext.SetRawBytesResponse` メソッドを使用すると、レスポンスとして生バイト配列を設定できます。これによりレスポンスのたびにシリアライズ処理を実行せずにキャッシュされたデータを効率的に送信できます。

これはゲームやアプリケーションの変わることの少ない初期データを返す場合などに有効です。

```csharp
public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
{
    if (ResponseBytesCache.TryGetValue(context.CallContext.Method, out var cachedBytes))
    {
        context.SetRawBytesResponse(cachedBytes);
        return;
    }

    await next(context);

    ResponseBytesCache[context.CallContext.Method] = MessagePackSerializer.Serialize(context.Result);
}
```

:::info
生成されたバイト列は MessagePack (またはカスタムシリアライズ) 形式でシリアライズされている必要があります。MagicOnion はバイト列を直接レスポンスバッファに書き込みます。
:::
