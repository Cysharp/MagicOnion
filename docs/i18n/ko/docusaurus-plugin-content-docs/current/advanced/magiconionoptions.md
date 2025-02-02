# MagicOnionOptions

`MagicOnionOptions`는 `MagicOnionEngine.BuildServerServiceDefinition(MagicOnionOptions option)`에 전달할 수 있습니다.

| 속성 | 설명 |
| --- | --- |
| `IList<MagicOnionFilterDescriptor>` GlobalFilters | 전역 MagicOnion 필터 |
| `bool` EnableCurrentContext | AsyncLocal을 통한 ServiceContext.Current 옵션 활성화, 기본값은 false |
| `IList<StreamingHubFilterDescriptor>` GlobalStreamingHubFilters | 전역 StreamingHub 필터 |
| `IGroupRepositoryFactory` DefaultGroupRepositoryFactory | StreamingHub를 위한 기본 GroupRepository 팩토리, 기본값은 `` |
| `bool` IsReturnExceptionStackTraceInErrorDetail | true인 경우, MagicOnion이 예외를 자체적으로 처리하여 메시지로 전송. false인 경우, gRPC 엔진으로 전파. 기본값은 false |
| `MessagePackSerializerOptions` SerializerOptions | MessagePack 직렬화 리졸버. 기본값은 ambient default(MessagePackSerializer.DefaultOptions) 사용 |
