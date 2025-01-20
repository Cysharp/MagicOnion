# MagicOnionOptions

`MagicOnionOptions` can pass to `MagicOnionEngine.BuildServerServiceDefinition(MagicOnionOptions option)`.

| Property | Description |
| --- | --- |
| `IList<MagicOnionFilterDescriptor>` GlobalFilters | Global MagicOnion filters. |
| `bool` EnableCurrentContext | Enable ServiceContext.Current option by AsyncLocal, default is false. |
| `IList<StreamingHubFilterDescriptor>` Global StreamingHub filters. | GlobalStreamingHubFilters |
| `IGroupRepositoryFactory` DefaultGroupRepositoryFactory | Default GroupRepository factory for StreamingHub, default is ``. |
| `bool` IsReturnExceptionStackTraceInErrorDetail | If true, MagicOnion handles exception ownself and send to message. If false, propagate to gRPC engine. Default is false. |
| `MessagePackSerializerOptions` SerializerOptions | MessagePack serialization resolver. Default is used ambient default(MessagePackSerializer.DefaultOptions). |
