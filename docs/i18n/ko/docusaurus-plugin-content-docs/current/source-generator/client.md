# Pre-generated client codes

MagicOnion은 Unity, NativeAOT 등 Pre-generated client codes가 필요한 플랫폼을 Source Generator로 지원합니다.

Source Generator는 MagicOnion.Client 패키지에 포함되어 있으므로, 이전 버전과 같은 코드 생성 도구(moc)의 설치나 추가 빌드 단계 설정이 불필요합니다.

## 지원되는 개발 환경
- Unity 2022.3.12f1 이상
- .NET 6 이상
- Visual Studio 2022 version 17.2 이상
- Rider 2023.1 이상

## 사용 방법
임의의 이름의 `partial` 클래스를 애플리케이션 내에 정의합니다. 그리고 `MagicOnionClientGeneration` 속성을 붙이고, 서비스 인터페이스를 검색할 대상이 되는 어셈블리 내의 임의의 타입을 지정합니다.

예를 들어 `MyApp.Shared` 어셈블리에 `MyApp.Shared.Services.IGreeterService`와 `MyApp.Shared.Hubs.IChatHub`가 포함되어 있는 경우 그 중 하나를 지정합니다.

```csharp
using MagicOnion.Client;

[MagicOnionClientGeneration(typeof(MyApp.Shared.Services.IGreeterService))]
partial class MagicOnionGeneratedClientInitializer {}
```

MagicOnion은 애플리케이션이 시작될 때, 생성된 클라이언트의 인수 타입과 반환 타입에 대응하는 포매터 팩터리를 등록합니다. 이 팩터리는 MessagePack의 `Default` 및 `DefaultAot` 옵션에서 사용할 수 있습니다. Native AOT 또는 IL2CPP에서 사용자 정의 제네릭 객체를 직렬화 루트로 사용하는 경우, 필요한 닫힌 타입을 `MessagePackSerializable`로 선언합니다. 자세한 내용은 [MessagePack v4 마이그레이션 가이드](https://github.com/MessagePack-CSharp/MessagePack-CSharp/tree/v4.0.0-rc.1#migrating-from-v3)를 참조하세요.

## 소스 생성 옵션
생성자의 명명된 인자로 옵션을 지정할 수 있습니다.

- **`DisableAutoRegistration`**: 시작 시 `<MagicOnionClientGeneration속성을 추가한 클래스>.Register` 메소드를 자동으로 호출하는 기능을 비활성화할지 여부를 설정합니다 (자동 등록은 .NET 5+ 또는 Unity에서만 작동합니다)
    - 이 옵션으로 `Register`를 호출하지 않은 경우, 수동으로 호출하거나 클라이언트 생성 시 `ClientFactoryProvider` 또는 `StreamingHubClientFactoryProvider` 프로퍼티를 전달해야 합니다
- **`Serializer`**: 직렬화에 사용할 시리얼라이저를 설정합니다. 기본값은 `GenerateSerializerType.MessagePack`입니다

### 추가 옵션

`MagicOnionClientGenerationOption` 속성으로 추가 옵션을 지정할 수 있습니다.

MessagePack 옵션을 사용하여 사용자 정의 `MessagePackFormatterFactory` 클래스를 등록할 수 있습니다.

- **`MessagePack.FormatterNamespace`**는 사용자 정의 포매터 팩터리의 루트 네임스페이스를 지정합니다. 기본값은 `MessagePack.Formatters`입니다.
- **`MessagePack.GenerateResolverForCustomFormatter`**를 활성화하면 제네릭 RPC 타입에 대한 `<TypeName>FormatterFactory<T>`가 등록됩니다. 기본값은 `false`입니다.
