# 패키지 설치 가이드

MagicOnion은 4개의 NuGet 패키지로 제공됩니다. 필요에 따라 해당하는 패키지를 설치하시기 바랍니다.

## MagicOnion.Server
`MagicOnion.Server` 패키지는 서버를 구현하기 위한 패키지입니다. 서버에서 서비스를 구현하기 위해서는 이 패키지를 설치해야 합니다.

```bash
dotnet add package MagicOnion.Server
```

## MagicOnion.Client
`MagicOnion.Client` 패키지는 클라이언트를 구현하기 위한 패키지입니다. 마이크로서비스나 WPF, .NET MAUI 등의 클라이언트를 구현하기 위해서는 이 패키지를 설치해야 합니다.

```bash
dotnet add package MagicOnion.Client
```

:::tip
Unity 환경에서 MagicOnion 클라이언트를 사용하려면 [Unity 환경에서 사용하기](unity)를 참고하세요.
:::

## MagicOnion.Abstractions
`MagicOnion.Abstractions` 패키지는 서버와 클라이언트에서 공통으로 사용되는 인터페이스와 속성을 제공합니다. 서버와 클라이언트 간에 공유되는 클래스 라이브러리 프로젝트를 생성하는 경우 이 패키지를 설치해야 합니다.

```bash
dotnet add package MagicOnion.Abstractions
```

## MagicOnion (메타 패키지)
`MagicOnion` 패키지는 서버와 클라이언트 역할을 모두 구현할 때 사용할 수 있는 메타 패키지입니다.
마이크로서비스와 같은 서버와 서버 간 통신을 구현하는 서버와 클라이언트를 모두 구현하는 경우 이 패키지를 설치하는 것이 좋습니다.

```bash
dotnet add package MagicOnion
```
