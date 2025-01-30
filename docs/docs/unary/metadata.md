# Metadata and headers

The request and response of Unary services can include metadata. Metadata is treated as HTTP headers, and can be set and read by both clients and servers. Metadata is represented by the `Metadata` class in gRPC.

This is useful when you need to add additional information to requests and responses, such as header authentication mechanisms or version information.

## Client

### Adding metadata to a request
To add metadata to a request, use the `WithHeaders` method of `MagicOnionClient`. The following example uses the `Metadata` class to add an `Authorization` header to the request.

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel).WithHeaders(new Metadata
    { "authorization", "Bearer {token}" }
});
```

You can also set `CallOptions` by passing the `MagicOnionClientOptions` class when creating a client, or using the `MagicOnionClient.WithOptions` method. Additionally, you can set metadata at the time of the request using [client filters](/filter/client-filter).

### Reading metadata from a response
You can read metadata using the `ResponseHeadersAsync` property of the `UnaryResult` struct. `ResponseHeadersAsync` waits for the server to respond.

The following is an example of reading metadata from a response.

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel);
var result = client.SayHelloAsync("Alice", 18);

var headers = await result.ResponseHeadersAsync;
```


## Server
You can also read the metadata of requests received from clients on the server, and add metadata to responses.

### Reading metadata from a request

You can read the metadata of a request using the `CallContext.RequestHeaders` property of the `ServiceContext` class.

```csharp
if (Context.CallContext.RequestHeaders.GetValue("authorization") is {} authorizationHeader)
{
    ...
}
```

### Adding metadata to a response

You can add metadata to a response using the `CallContext.WriteResponseHeadersAsync` method of the `ServiceContext` class.

```csharp
Context.CallContext.WriteResponseHeadersAsync(new Metadata
{
    { "x-server-version", "1.0.0" }
});
```

In addition, it is also possible to set values as HTTP headers from `HttpContext`, which is a standard method in ASP.NET Core.

```csharp
Context.CallContext.GetHttpContext().Response.Headers.TryAdd("x-server-version", "1.0.0");
```
