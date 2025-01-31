# Raw bytes response
In a filter context, `ServiceContext.SetRawBytesResponse` method allows you to set raw byte sequences as a response. This makes it possible to send a cached response body without serialization.

This is useful when returning initial data that changes infrequently in games or applications.

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

> [!NOTE]
> The raw byte sequence must be serialized as a MessagePack (or custom serialization) format. MagicOnion will write a byte requence directly into the response buffer.
