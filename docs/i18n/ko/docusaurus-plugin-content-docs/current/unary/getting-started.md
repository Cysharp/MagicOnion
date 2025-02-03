# Unary 서비스 시작하기

이 튜토리얼에서는 Unary 서비스의 구현을 시작하기 위한 간단한 절차를 소개합니다.

## 절차

Unary 서비스를 정의, 구현, 이용하기 위해서는 아래의 절차가 필요합니다.

- 서버와 클라이언트 간에 공유할 Unary 서비스 인터페이스를 정의합니다.
- 서버 프로젝트에서 정의한 Unary 서비스 인터페이스를 구현합니다.
- 클라이언트 프로젝트에서 정의한 Unary 서비스를 호출합니다.

## 서버와 클라이언트 간에 공유할 Unary 서비스 인터페이스를 정의

공유 라이브러리 프로젝트에 Unary 서비스의 인터페이스를 정의합니다 (Unity의 경우는 소스 코드 복사나 파일 링크로 대응합니다). Unary 서비스의 인터페이스는 `IService<TSelf>`를 상속해야 합니다.

다음은 인사를 반환하는 Unary 서비스 인터페이스의 예시입니다.

```csharp
public interface IGreeterService : IService<IGreeterService>
{
    UnaryResult<string> SayHelloAsync(string name, int age);
}
```

Unary 서비스에 정의하는 메서드의 반환값 타입은 `UnaryResult` 또는 `UnaryResult<T>`여야 합니다. 이는 `ValueTask`, `ValueTask<T>`와 같은 의미를 가지는, Unary 서비스 특유의 반환값 타입입니다.

## 서버 프로젝트에서 정의한 Unary 서비스 인터페이스를 구현

서버 상에서 클라이언트로부터 호출되는 Unary 서비스를 구현합니다. 서버의 구현은 `ServiceBase<TSelf>`를 상속하고, 정의한 Unary 서비스 인터페이스를 구현해야 합니다.

```csharp
public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public async UnaryResult<string> SayHelloAsync(string name, int age)
    {
        return $"Hello {name}! Your age is {age}.";
    }
}
```

`UnaryResult` 및 `UnaryResult<T>` 타입은 `ValueTask`, `Task` 등과 마찬가지로 비동기 메서드(`async`)로 정의할 수 있습니다.

메서드의 처리에 비동기가 필요하지 않은 경우에는 `UnaryResult.FromResult`나 `UnaryResult.CompletedResult` 를 사용하여 동기적으로 반환할 수도 있습니다.


```csharp
public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public UnaryResult<string> SayHelloAsync(string name, int age)
    {
        return UnaryResult.FromResult($"Hello {name}! Your age is {age}.");
    }
}
```

## 클라이언트 프로젝트에서 정의한 Unary 서비스를 호출

클라이언트에서 Unary 서비스의 메서드를 호출하려면 `MagicOnionClient.Create<T>` 메서드를 사용합니다.

`Create<T>` 메서드에는 Unary 서비스 인터페이스와 접속할 `GrpcChannel` 객체를 전달합니다. 이 메서드는 지정한 인터페이스에 대응하는 클라이언트 프록시를 생성합니다. 이 시점에서는 서버에 요청이 발생하지 않습니다.

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = await MagicOnionClient.Create<IGreeterService>(channel, receiver);
```

생성한 클라이언트 프록시를 사용하여 Unary 서비스의 메서드를 호출합니다.

```csharp
var result = await client.SayHelloAsync("Alice", 18);
Console.WriteLine(result);
```
