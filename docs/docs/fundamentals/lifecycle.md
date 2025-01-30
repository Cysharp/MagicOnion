# ServiceContext and Lifecycle

## ServiceContext

In Unary service and StreamingHub method, and inside filters, you can access `this.Context` to get `ServiceContext`.

| Property | Type | Description |
| --- | --- | --- |
| Items | `ConcurrentDictionary<string, object>` | Object storage per request/connection. |
| ContextId | `Guid` | Unique ID per request(Service)/connection(StreamingHub). |
| Timestamp | `DateTime` | Timestamp that request/connection is started time. |
| ServiceType | `Type` | Invoked Class. |
| MethodInfo | `MethodInfo` | Invoked Method. |
| AttributeLookup | `ILookup<Type, Attribute>` | Cached Attributes that merged both service and method. |
| CallContext | `ServerCallContext` | Raw gRPC Context |
| MessageSerializer | `IMagicOnionSerializer` | Using serializer. |
| ServiceProvider | `IServiceProvider` | Get the service provider. |

You can use `Items` to set values from authentication filters and retrieve them from service methods.

:::warning
**DO NOT cache ServiceContext.** ServiceContext is only valid during the request and MagicOnion may reuse instances. The state of objects referenced from the context after the request is also undefined.
:::

:::warning
ServiceContext is a "per connection" context. You can access ServiceContext inside StreamingHub, but be aware that the same context is shared throughout the connection. For example, Timestamp is the time when the connection was established, and properties related to methods are always set by special methods such as `Connect`. The `Items` property is not cleared per Hub method invocation.

Inside StreamingHubFilter, use StreamingHubContext. StreamingHubContext is the context for each StreamingHub method invocation.
:::

### Global ServiceContext
In MagicOnion, you can get the current context globally like `HttpContext.Current`. You can get it with `ServiceContext.Current`, but `MagicOnionOptions.EnableCurrentContext = true` is required. The default is `false`.

For reasons of performance and code complexity, it is recommended to avoid using `ServiceContext.Current`.

## Lifecycle

The lifecycle of a Unary service is as follows in pseudo code. A new service instance is created each time a request is received and processed.

```csharp
async Task<Response> UnaryMethod(Request request)
{
    var service = new ServiceImpl();
    var context = new ServiceContext();
    context.Request = request;

    var response = await Filters.Invoke(context, (args) =>
    {
        service.ServiceContext = context;
        return await service.MethodInvoke(args);
    });

    return response;
}
```

The lifecycle of a StreamingHub service is as follows in pseudo code. While connected, the StreamingHub instance is maintained, so state can be maintained.

```csharp
async Task StreamingHubMethod()
{
    var context = new ServiceContext();

    var hub = new StreamingHubImpl();
    hub.ServiceContext = context;

    await Filters.Invoke(context, () =>
    {
        var streamingHubContext = new StreamingHubContext(context);
        while (connecting)
        {
            var message = await ReadHubInvokeMessageFromStream();
            streamingHubContext.Intialize(context);

            await StreamingHubFilters.Invoke(streamingHubContext, () =>
            {
                return await hub.MethodInvoke();
            });
        }
    });
}
```
