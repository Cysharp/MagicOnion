# 원시 바이트 응답
필터 컨텍스트에서, `ServiceContext.SetRawBytesResponse` 메서드는 응답으로 원시 바이트 시퀀스를 설정할 수 있게 합니다. 이는 직렬화 없이 캐시된 응답 본문을 보낼 수 있게 합니다.

이는 게임이나 애플리케이션에서 자주 변경되지 않는 초기 데이터를 반환할 때 유용합니다.

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
원시 바이트 시퀀스는 반드시 MessagePack (또는 사용자 지정 직렬화) 형식으로 직렬화되어야 합니다. MagicOnion은 바이트 시퀀스를 응답 버퍼에 직접 기록할 것입니다.
:::
