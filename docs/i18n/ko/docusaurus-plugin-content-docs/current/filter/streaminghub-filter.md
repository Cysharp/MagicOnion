# StreamingHub 필터

:::info
이 기능과 문서는 서버 측에만 적용됩니다.
:::

StreamingHub에도 필터를 적용할 수 있지만 필터의 종류와 동작에 주의가 필요합니다. 이 페이지에서는 StreamingHub에서의 필터의 기본적인 사용법과 주의점에 대해 설명합니다.

## Unary 서비스용 필터 사용하기
Unary 서비스용으로 구현한 필터 MagicOnionFilter를 StreamingHub에 적용할 수 있습니다. 단, StreamingHub에 적용한 경우 필터가 실행되는 것은 연결 시(`Connect`)에만 해당됩니다. 연결 후의 **Hub 메소드 호출 시에는 필터가 실행되지 않습니다**. 따라서 적용할 수 있는 것은 클래스 또는 글로벌에만 해당됩니다.

이는 StreamingHub의 연결 상태 메트릭스나 인증과 같은 것을 다루는 경우에는 적합하지만, Hub 메소드마다 후크(Hook)하고 싶은 경우에는 다음에 설명하는 StreamingHub 필터를 사용해 주세요.

:::tip
Unary용 필터를 글로벌 필터로 설정한 경우, StreamingHub에도 적용된다는 점에 주의가 필요합니다. 예를 들어 메소드의 실행 시간을 측정하는 등의 경우 StreamingHub에 의해 의도치 않게 Connect가 기록되는 등의 일이 발생할 수 있습니다.
:::

## StreamingHub 필터 사용하기
StreamingHub 필터는 Hub 메소드의 호출 전후에 후크(Hook)하는 필터입니다. Unary 서비스의 필터와 거의 같지만 MagicOnionFilterAttribute 대신 StreamingHubFilterAttribute를 상속하여 `Invoke` 메소드를 구현합니다.

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

StreamingHub 필터는 Hub 메소드의 호출 단위로 실행되므로, 에러 핸들링이나 로깅, 메소드의 측정 등에 적합합니다.

StreamingHub 필터도 일반 필터와 마찬가지로 확장용 인터페이스가 준비되어 있어서, 유연한 필터 구현이 가능합니다. 자세한 내용은 [필터의 확장성](/filter/extensibility) 페이지를 참조하시기 바랍니다.
