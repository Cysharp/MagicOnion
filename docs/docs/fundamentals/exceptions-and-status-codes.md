# Handling Exceptions and status codes

MagicOnion allows you to return the result of server processing as a status code separately from the return value. This is similar to HTTP status codes or gRPC status codes. You can also include application-specific status codes in these status codes.

## Notifying the client of the status code from the server
If you want to return a custom status code from the server to the client, you can use `ReturnStatusException`.

```csharp
public Task SendMessageAsync(string message)
{
    if (message.Contains("foo"))
    {
        //
        throw new ReturnStatusException((Grpc.Core.StatusCode)MyStatusCode.SomethingWentWrong, "invalid");
    }

    // ....
```

The `ReturnStatusException` has an `Grpc.Core.StatusCode` enumeration type as the status code, and this value is notified to the client. To return a custom status code for the application, cast your own `int` value or enumeration type to the `Grpc.Core.StatusCode` enumeration type.

If you are performance-centric and want to avoid throwing exceptions, you can use `CallContext.Status` (`ServiceContext.CallContext.Status`) to set the status directly.

## Exception handling on the client
All exceptions that occur when a method is called on the client are received as a `RpcException` of gRPC.

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel);
try
{
    var result = await client.SayHelloAsync("Alice", 18);
}
catch (RpcException ex)
{
    // handle exception ...
    if (((MyStatusCode)ex.Status.StatusCode) == MyStatusCode.SomethingWentWrong)
    {
        // ...
    }
}
```

If there is a problem with the network, `RpcException` is thrown with `StatusCode.Unavailable`. You can investigate the detailed error content with the `InnerException` property.

## Unhandled exceptions on the server
If an unhandled exception is thrown during a method call on the server, except for `ReturnStatusException`, the client treats it as an `RpcException` with `StatusCode.Unknown`.

If `MagicOnionOption.IsReturnExceptionStackTraceInErrorDetail` is `true`, the client can receive the stack trace of the server exception. This is very useful for debugging, but it has a critical security issue, so it should only be enabled in debug builds.

