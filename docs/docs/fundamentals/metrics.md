# Metrics
MagicOnion server supports metrics related to StreamingHub using System.Diagnostics.Metrics.

- see: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics

## Meter: MagicOnion.Server

|Metric|Unit|Tags|
|--|--|--|
|magiconion.server.streaminghub.connections|`{connection}`|`rpc.system`, `rpc.service`|
|magiconion.server.streaminghub.method_duration|`ms`|`rpc.system`, `rpc.service`, `rpc.method`|
|magiconion.server.streaminghub.method_completed|`{request}`|`rpc.system`, `rpc.service`, `rpc.method`, `magiconion.streaminghub.is_error`|
|magiconion.server.streaminghub.exceptions|`{exception}`|`rpc.system`, `rpc.service`, `rpc.method`, `error.type`|

## Tags

|Tag name|Value|
|--|--|
|rpc.system|`magiconion`|
|rpc.service|StreamingHub interface name (e.g. `IGreeterService`)|
|rpc.method|StreamingHub method name (e.g. `HelloAsync`)|
|magiconion.streaminghub.is_error|Whether a StreamingHub method call succeeded or failed.  (e.g. `true` or `false`)|
|error.type|Thrown exception type (e.g. `System.InvalidOperationException`)|
