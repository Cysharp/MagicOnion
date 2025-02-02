# 메트릭
MagicOnion 서버는 System.Diagnostics.Metrics를 사용한 StreamingHub 관련 메트릭(Metrics)을 지원합니다.

- see: https://learn.microsoft.com/ko-kr/dotnet/core/diagnostics/metrics

## Meter: MagicOnion.Server

|Metric|Unit|Tags|
|--|--|--|
|magiconion.server.streaminghub.connections|`{connection}`|`rpc.system`, `rpc.service`|
|magiconion.server.streaminghub.method_duration|`ms`|`rpc.system`, `rpc.service`, `rpc.method`|
|magiconion.server.streaminghub.method_completed|`{request}`|`rpc.system`, `rpc.service`, `rpc.method`, `magiconion.streaminghub.is_error`|
|magiconion.server.streaminghub.exceptions|`{exception}`|`rpc.system`, `rpc.service`, `rpc.method`, `error.type`|

## Tags

|Tag name|Value|
|--|---------------------------------------------------|
|rpc.system|`magiconion`|
|rpc.service|StreamingHub 인터페이스 이름 (예: `IGreeterService`)|
|rpc.method|StreamingHub 메소드명 (예: `HelloAsync`)|
|magiconion.streaminghub.is_error|StreamingHub 메서드 호출이 실패했는지 여부 (예: `true` or `false`)|
|error.type|발생한 예외의 타입 (예: `System.InvalidOperationException`)|
