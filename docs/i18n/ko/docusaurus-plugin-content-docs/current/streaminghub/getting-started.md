# StreamingHub 시작하기

이 튜토리얼에서는 StreamingHub를 시작하기 위한 간단한 순서를 소개합니다.

## 절차

StreamingHub를 정의, 구현, 이용하기 위해서는 아래의 순서가 필요하게 됩니다.

- 서버와 클라이언트 사이에서 공유하는 StreamingHub 인터페이스를 정의합니다.
- 서버 프로젝트에서 정의한 StreamingHub 인터페이스를 구현합니다.
- 클라이언트 프로젝트에서 정의한 StreamingHub 수신자(receiver)를 구현합니다.
- 클라이언트 프로젝트에서 정의한 StreamingHub를 호출하기 위한 클라이언트 프록시를 작성합니다.

## 서버와 클라이언트 사이에서 공유하는 StreamingHub 인터페이스 정의

공유 라이브러리 프로젝트에 StreamingHub의 인터페이스를 정의합니다 (Unity의 경우는 소스코드 복사나 파일 링크로 대응합니다).

StreamingHub의 인터페이스는 `IStreamingHub<TSelf, TReceiver>`를 상속할 필요가 있습니다. `TSelf`에는 인터페이스 자신, `TReceiver`에는 receiver 인터페이스를 지정합니다. receiver 인터페이스는 서버에서 클라이언트로 메시지를 송신하고, 수신하기 위한 인터페이스입니다.

다음은 채팅 애플리케이션의 StreamingHub 인터페이스의 예입니다. 클라이언트는 메시지의 수신이나 참가, 퇴장 이벤트를 보내는 receiver 인터페이스를 가지고 있습니다.

```csharp
// A hub must inherit `IStreamingHub<TSelf, TReceiver>`.
public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
{
    ValueTask JoinAsync(string roomName, string userName);
    ValueTask LeaveAsync();
    ValueTask SendMessageAsync(string message);
}

public interface IChatHubReceiver
{
    void OnJoin(string userName);
    void OnLeave(string userName);
    void OnMessage(string userName, string message);
}
```

StreamingHub가 제공하는 메소드를 Hub 메소드라고 부릅니다. Hub 메소드는 클라이언트에서 호출되는 메소드로, 반환값의 타입은 `ValueTask`, `ValueTask<T>`, `Task`, `Task<T>`, `void` 중 하나여야 합니다. Unary 서비스와는 다르다는 것에 주의가 필요합니다.

클라이언트가 메시지를 받는 입구가 되는 receiver 인터페이스 또한 메소드를 가집니다. 이것들을 **receiver 메소드**라고 부릅니다. receiver 메소드는 서버에서 메시지를 받았을 때 호출되는 메소드입니다. receiver 메소드의 반환값은 `void`여야 합니다. 클라이언트 결과를 사용하는 경우를 제외하고, 원칙적으로 `void`를 지정합니다.

## 서버 프로젝트에서 StreamingHub 구현하기

서버 상에 클라이언트에서 호출할 수 있는 StreamingHub를 구현할 필요가 있습니다. 서버 구현은 `StreamingHubBase<THub, TReceiver>`를 상속하고, 정의한 StreamingHub 인터페이스를 구현할 필요가 있습니다.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    public async ValueTask JoinAsync(string roomName, string userName)
        => throw new NotImplementedException();

    public async ValueTask LeaveAsync()
        => throw new NotImplementedException();

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

처음에 채팅룸에 참가하는 메소드 `JoinAsync`를 구현합니다. 이 메소드는 지정된 이름의 룸에 지정된 사용자 이름으로 참가합니다.

`Group.AddAsync` 메소드로 그룹을 생성하고, 그 그룹에 대한 참조를 StreamingHub에 보관하여 이후의 처리에서 사용합니다. 그룹의 `All` 속성을 통해 그룹에 참가하고 있는 클라이언트의 receiver 인터페이스를 얻을 수 있으므로 `OnJoin` 메소드를 호출하여 참가를 통지합니다.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);
    }

    public async ValueTask LeaveAsync()
        => throw new NotImplementedException();

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

`Client` 속성을 사용하면 그 StreamingHub에 접속하고 있는 클라이언트만을 호출할 수도 있습니다. 여기서는 접속해온 클라이언트에게만 환영 메시지를 송신해보겠습니다.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);

        Client.OnMessage("System", $"Welcome, hello {userName}!");
    }

    public async ValueTask LeaveAsync()
        => throw new NotImplementedException();

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

다음으로 퇴장하는 메소드 `LeaveAsync`도 구현합니다. 퇴장 시에는 그룹에서 클라이언트를 삭제합니다. 이것은 `Group.RemoveAsync` 메소드를 사용하여 수행합니다. `RemoveAsync` 메소드에는 `StreamingHubContext` 클래스의 오브젝트(`Context` 속성)를 전달합니다. 그룹에서 클라이언트가 삭제되면 그룹을 통한 메시지가 그 클라이언트에게는 도달하지 않게 됩니다.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);
    }

    public async ValueTask LeaveAsync()
    {
        room.All.OnLeave(ConnectionId.toString());
        await room.RemoveAsync(Context);
    }

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

마지막으로 클라이언트에서 메시지를 받으면 그룹에 배포하는 `SendMessageAsync` 메소드를 구현합니다. 이 메소드에서는 그룹의 `All` 속성을 통해 그룹에 참가하고 있는 클라이언트의 `OnMessage` 메소드를 호출하여 통지합니다.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);
    }

    public async ValueTask LeaveAsync()
    {
        room.All.OnLeave(userName);
        await room.RemoveAsync(Context);
    }

    public async ValueTask SendMessageAsync(string message)
    {
        room.All.OnMessage(userName, message);
    }
}
```

## 클라이언트 프로젝트에서 StreamingHub receiver 구현하기

클라이언트 프로젝트에서 StreamingHub의 receiver 인터페이스를 구현합니다. 이 인터페이스는 클라이언트 측에서 메시지를 받아, 처리하고 싶은 타입에 구현합니다.

여기서는 심플한 ChatHubReceiver 타입을 작성하고, receiver 인터페이스 `IChatHubReceiver`를 구현합니다. 각각의 메소드는 서버에서 송신된 메시지를 받아 콘솔에 메시지를 출력합니다.

```csharp
class ChatHubReceiver : IChatHubReceiver
{
    public void OnJoin(string userName)
        => Console.WriteLine($"{userName} joined.");
    public void OnLeave(string userName)
        => Console.WriteLine($"{userName} left.");
    public void OnMessage(string userName, string message)
        => Console.WriteLine($"{userName}: {message}");
}
```

## 클라이언트에서 StreamingHub에 접속하여 메소드를 호출하기

클라이언트에서 StreamingHub에 접속하기 위해서는 `StreamingHubClient.ConnectAsync` 메소드를 사용합니다. 이 메소드는 접속을 확립하고, 클라이언트 프록시를 반환합니다.

`ConnectAsync` 메소드에는 접속할 `GrpcChannel` 오브젝트와 receiver 인터페이스의 인스턴스를 전달합니다. 접속이 확립되면 클라이언트 프록시가 반환됩니다. 서버에서 수신한 메시지는 여기서 전달한 receiver의 인스턴스의 메소드 호출이 됩니다.

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var receiver = new ChatHubReceiver();
var client = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver);

await client.JoinAsync("room", "user1");
await client.SendMessageAsync("Hello, world!");
```
