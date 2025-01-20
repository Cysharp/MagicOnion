# メトリクス
MagicOnion サーバーは、System.Diagnostics.Metrics を使用した StreamingHub に関連するメトリクスをサポートしています。

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
|rpc.service|StreamingHub インターフェース名 (例: `IGreeterService`)|
|rpc.method|StreamingHub メソッド名 (例: `HelloAsync`)|
|magiconion.streaminghub.is_error|StreamingHub メソッドの呼び出しが失敗したかどうか (例: `true` or `false`)|
|error.type|スローされた例外の型 (例: `System.InvalidOperationException`)|
