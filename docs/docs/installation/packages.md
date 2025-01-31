# Package Installation Guide

MagicOnion is available in four NuGet packages. Please install any of the packages as needed.

## MagicOnion.Server
The package `MagicOnion.Server` to implement the server. You need to install this package to implement services on your server.

```bash
dotnet add package MagicOnion.Server
```

## MagicOnion.Client
The package `MagicOnion.Client` to implement the client. To implement the client such as as Console, WPF and .NET MAUI, you need to install this package.

```bash
dotnet add package MagicOnion.Client
```

:::tip
If you want to use MagicOnion client with Unity clients, see also [Works with Unity](unity) page.
:::

## MagicOnion.Abstractions
The package `MagicOnion.Abstractions` provides interfaces and attributes commonly used by servers and clients. To create a class library project which is shared between the servers and the clients, you need to install this package.

```bash
dotnet add package MagicOnion.Abstractions
```

## MagicOnion (meta package)
The package `MagicOnion` is meta package to implements the role of both server and client.
To implement server-to-server communication such as Microservices, that can be both a server and a client, we recommend to install this package.

```bash
dotnet add package MagicOnion
```
