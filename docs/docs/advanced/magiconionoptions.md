# MagicOnionOptions

Configure `MagicOnionOptions` with `services.AddMagicOnion(options => { ... })` or the `MagicOnion` section in configuration, such as `appsettings.json`.

| Property | Description |
| --- | --- |
| `IList<MagicOnionFilterDescriptor>` GlobalFilters | Global MagicOnion filters. |
| `bool` EnableCurrentContext | Enable ServiceContext.Current option by AsyncLocal, default is false. |
| `IList<StreamingHubFilterDescriptor>` Global StreamingHub filters. | GlobalStreamingHubFilters |
| `IGroupRepositoryFactory` DefaultGroupRepositoryFactory | Default GroupRepository factory for StreamingHub, default is ``. |
| `bool` IsReturnExceptionStackTraceInErrorDetail | If true, MagicOnion handles exception ownself and send to message. If false, propagate to gRPC engine. Default is false. |
| `MessagePackSerializerOptions` SerializerOptions | MessagePack serialization resolver. Default is used ambient default(MessagePackSerializer.DefaultOptions). |
| `int?` StreamingHubResponseQueueMaxLength | Maximum number of responses waiting in each StreamingHub connection's response queue. Default is 1,024; `null` disables the count limit. |
| `long?` StreamingHubResponseQueueMaxSize | Maximum total serialized payload size in bytes waiting in each StreamingHub connection's response queue. Default is 16 MiB (16,777,216 bytes); `null` disables the size limit. |

For example, configure the response queue limits in `appsettings.json`:

```json
{
  "MagicOnion": {
    "StreamingHubResponseQueueMaxLength": 1024,
    "StreamingHubResponseQueueMaxSize": 16777216
  }
}
```

Both limits must be positive when set. They apply to all queued messages for a connection, including Hub method responses, broadcasts, and heartbeats. The response already handed to gRPC is excluded. The size limit counts payload data lengths, not pooled buffer capacities or gRPC/HTTP buffers.

When accepting a response would exceed either limit, MagicOnion stops accepting responses and aborts that connection's gRPC call. The rejected response and pending responses are returned to their pool. A single response larger than the size limit also triggers this behavior, even if the queue is empty.
