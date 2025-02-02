# 클라이언트 호출

StreamingHub에서는 서버에서 클라이언트 수신자(receiver)에 대해 메시지를 송신할 수 있습니다. 이 메시지의 송신은 서버상에서 receiver의 프록시가 되는 receiver 인터페이스의 메소드(receiver 메소드)를 호출함으로써 수행합니다.

메시지의 송신에는 크게 2가지 방법이 있습니다. 하나는 StreamingHub의 인스턴스에 연결되어 있는 클라이언트를 호출하는 방법, 다른 하나는 그룹에 속한 클라이언트에 일제히 송신하는 방법입니다.

## StreamingHub 인스턴스에 연결된 클라이언트를 호출하기

StreamingHub의 인스턴스에 연결되어 있는 클라이언트에 메시지를 송신하려면, `Client` 속성을 사용합니다. 이 속성은 receiver 인터페이스를 구현한 클라이언트의 프록시를 제공합니다.

```csharp
public async Task EchoAsync(string message)
{
    this.Client.OnMessage("Echo: " + message);
}
```

## 그룹에 속한 클라이언트에 일제히 송신

그룹에 속한 클라이언트에 일제히 메시지를 송신하려면, 그룹의 취득 또는 생성을 해서 그룹의 인스턴스를 취득할 필요가 있습니다. 이 그룹을 사용한 송신에 대해서는 [그룹 기본 사항](group)를 참조해 주세요.
