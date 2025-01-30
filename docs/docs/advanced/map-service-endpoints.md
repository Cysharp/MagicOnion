# Map Service Endpoints

## Map specific types or services in an assembly

By default, MagicOnion server automatically searches for services contained in the assembly that is started and registers and exposes all found services. However, in some cases, you may want to expose only specific types or types contained in specific assemblies.

`MapMagicOnionService` method has an overload that maps only services contained in specific types or assemblies. By specifying this overload, you can manually register services.

```csharp
app.MapMagicOnionService([ typeof(MyService), typeof(MyHub) ]);
app.MapMagicOnionService([ typeof(MyService).Assembly ]);
```

## Setting endpoint metadata

`MapMagicOnionService` method returns a builder that allows you to set ASP.NET Core endpoint metadata. For example, methods such as `RequireHost` and `RequireAuthorization` are available.

It is possible to provide different services on multiple ports as shown below.

```csharp
// Consumers endpoints
app.MapMagicOnionService([typeof(GreeterService), typeof(ChatHub)]);

// Administration endpoints
app.MapMagicOnionService([typeof(AdministrationService)])
    .RequireHost("*:6000")
    .RequireAuthorization();
```
