# ServiceContext와 Lifecycle

## ServiceContext

Unary 서비스와 StreamingHub 메서드, 그리고 필터 내부에서 `this.Context`를 통해 `ServiceContext`에 접근할 수 있습니다.

| 속성 | 타입 | 설명 |
| --- | --- | --- |
| Items | `ConcurrentDictionary<string, object>` | 요청(request)/연결(connection)당 객체 저장소입니다. |
| ContextId | `Guid` | 요청(서비스)/연결(StreamingHub)당 고유 ID입니다. |
| Timestamp | `DateTime` | 요청/연결이 시작된 시간의 타임스탬프입니다. |
| ServiceType | `Type` | 호출된 클래스입니다. |
| MethodInfo | `MethodInfo` | 호출된 메서드입니다. |
| AttributeLookup | `ILookup<Type, Attribute>` | 서비스와 메서드 모두에서 병합된 캐시된 속성들입니다. |
| CallContext | `ServerCallContext` | Raw gRPC 컨텍스트입니다. |
| MessageSerializer | `IMagicOnionSerializer` | 사용 중인 시리얼라이저입니다. |
| ServiceProvider | `IServiceProvider` | 서비스 제공자를 가져옵니다. |

`Items`는 인증 필터와 같은 곳에서 값을 설정하고, 서비스 메서드에서 가져오기 위해 사용할 수 있습니다.

:::warning
**ServiceContext를 캐시하지 마십시오.** ServiceContext는 요청 중에만 유효하며 MagicOnion은 인스턴스를 재사용할 수 있습니다. 요청 후 컨텍스트에서 참조된 객체들의 상태도 정의되지 않습니다.
:::

:::warning
ServiceContext는 "연결당" 컨텍스트입니다. StreamingHub 내부에서 ServiceContext에 접근할 수 있지만, 동일한 컨텍스트가 연결 전체에서 공유된다는 점을 주의하세요. 예를 들어, Timestamp는 연결이 설정된 시간이며, 메서드와 관련된 속성들은 항상 `Connect`와 같은 특별한 메서드에 의해 설정됩니다. `Items` 속성은 Hub 메서드 호출마다 초기화되지 않습니다.

StreamingHubFilter 내부에서는 StreamingHubContext를 사용하세요. StreamingHubContext는 각 StreamingHub 메서드 호출에 대한 컨텍스트입니다.
:::

### 전역(global) ServiceContext
MagicOnion은 HttpContext.Current처럼 전역으로 현재의 컨텍스트를 가져올 수 있습니다. `ServiceContext.Current`로 가져올 수 있지만, `MagicOnionOptions.EnableCurrentContext = true`가 필요합니다. 기본값은 `false`입니다.

성능과 코드의 복잡성 관점에서 `ServiceContext.Current`의 사용은 피하는 것을 권장합니다.

## Lifecycle

Unary 서비스의 Lifecycle은 의사 코드로 다음과 같습니다. 요청이 수신되고 처리될 때마다 새로운 서비스 인스턴스가 생성됩니다.

```csharp
async Task<Response> UnaryMethod(Request request)
{
    var service = new ServiceImpl();
    var context = new ServiceContext();
    context.Request = request;

    var response = await Filters.Invoke(context, (args) =>
    {
        service.ServiceContext = context;
        return await service.MethodInvoke(args);
    });

    return response;
}
```

StreamingHub 서비스의 Lifecycle은 의사 코드로 다음과 같습니다. 연결되어 있는 동안 StreamingHub 인스턴스가 유지되므로 상태를 유지할 수 있습니다.

```csharp
async Task StreamingHubMethod()
{
    var context = new ServiceContext();

    var hub = new StreamingHubImpl();
    hub.ServiceContext = context;

    await Filters.Invoke(context, () =>
    {
        var streamingHubContext = new StreamingHubContext(context);
        while (connecting)
        {
            var message = await ReadHubInvokeMessageFromStream();
            streamingHubContext.Intialize(context);

            await StreamingHubFilters.Invoke(streamingHubContext, () =>
            {
                return await hub.MethodInvoke();
            });
        }
    });
}
```
