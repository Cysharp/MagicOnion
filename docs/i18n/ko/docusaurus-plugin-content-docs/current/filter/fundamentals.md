# 필터 기본 사항

:::info
이 기능과 문서는 서버 측에만 적용됩니다. 클라이언트 측의 필터를 사용하는 경우는, [클라이언트 필터](client-filter) 페이지를 참조하시기 바랍니다.
:::

MagicOnion은 서비스의 메소드 호출 전후에 후크(Hook)하는 필터라는 강력한 기능을 제공합니다. 필터는 gRPC 서버 인터셉터와 비슷한 기능을 제공하지만, HttpClient의 핸들러나 ASP.NET Core의 미들웨어와 같은 친숙한 프로그래밍 모델을 제공합니다.

![image](https://user-images.githubusercontent.com/46207/50969421-cb465900-1521-11e9-8824-8a34cc52bbe4.png)

아래 그림은 필터가 복수 설정되어, 처리되는 구성의 이미지입니다.

![image](https://user-images.githubusercontent.com/46207/50969539-2bd59600-1522-11e9-84ab-15dd85e3dcac.png)

## 구현과 사용 방법

필터의 구현은 `MagicOnionFilterAttribute`를 상속하여 `Invoke` 메소드를 구현합니다.

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

필터 내에서는 `next` 델리게이트를 호출함으로써 다음 필터 또는 실제 메소드를 호출합니다. 예를 들어 `next`의 호출을 스킵하거나, `next`의 호출의 예외를 캐치함으로써 예외 시의 처리를 추가하는 등의 것이 가능합니다.

구현한 필터를 적용하려면 Unary 서비스의 클래스 또는 메소드에 `[SampleFilter]`와 같이 속성을 부여합니다. 글로벌 필터로서 애플리케이션 전체에 적용도 가능합니다.

MagicOnion은 ASP.NET Core MVC와 매우 비슷한 필터의 API도 제공하고 있습니다.
이러한 API들은 유연한 필터의 구현을 지원합니다. 자세한 내용은 [필터의 확장성](extensibility) 페이지를 참조하시기 바랍니다.

## 글로벌 필터
필터는 MagicOnionOptions의 `GlobalFilters`에 추가함으로써 애플리케이션 전체에 적용할 수 있습니다.

```csharp
services.AddMagicOnion(options =>
{
    options.GlobalFilters.Add<MyServiceFilter>();
    options.GlobalStreamingHubFilters.Add<MyHubFilter>();
});
```

## 처리 순서
필터는 순서를 지정할 수 있으며, 다음 순서로 실행됩니다.

```
[Ordered filters] -> [Global filters] -> [Class filters] -> [Method filters]
```

순서가 지정되지 않은 필터는 마지막(`int.MaxValue`)으로 취급되며, 추가된 순서대로 실행됩니다.

## ASP.NET Core의 미들웨어와의 통합
MagicOnion 서버에서는 ASP.NET Core의 미들웨어도 사용할 수 있지만 필터보다 먼저 실행됩니다. 이는 gRPC의 호출이 순수한 HTTP 요청으로 처리되는 것에 비해, 필터는 gRPC의 호출로서 처리가 시작된 후에 실행되기 때문입니다.

<img src={require('/img/docs/fig-filter-with-middleware.png').default} alt="" style={{height: '320px'}} />
