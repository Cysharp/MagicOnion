## Table of Contents

* [server ops](#server-ops)
* [client ops](#client-ops)

## Basics
### build

```shell
# linux
dotnet publish benchmark/Benchmark.Server/Benchmark.Server.csproj -c Release -r linux-x64 -p:PublishSingleFile=true --no-self-contained -o benchmark/out/linux/server
dotnet publish benchmark/Benchmark.Client/Benchmark.Client.csproj -c Release -r linux-x64 -p:PublishSingleFile=true --no-self-contained -o benchmark/out/linux/client

# win
dotnet publish benchmark/Benchmark.Server/Benchmark.Server.csproj -c Release -r win-x64 -p:PublishSingleFile=true --no-self-contained -o benchmark/out/win/server
dotnet publish benchmark/Benchmark.Client/Benchmark.Client.csproj -c Release -r win-x64 -p:PublishSingleFile=true --no-self-contained -o benchmark/out/win/client
```

### run

run server
```shell
ASPNETCORE_ENVIRONMENT=Development ./benchmark/out/linux/server/Benchmark.Server
ASPNETCORE_ENVIRONMENT=Production sudo ./benchmark/out/linux/server/Benchmark.Server
```

run client

```shell
./out/linux/client/Benchmark.Client
```

emulate s3 with minio.

```shell
docker-compose -f benchmark/docker-compose.yaml up
BENCHCLIENT_EMULATE_S3=1 ./benchmark/out/linux/client/Benchmark.Client benchmarkrunner listreports --reportId 1
```

### push binary to s3 bucket

```
BUCKET=bench-magiconion-s3-bucket-5c7e45b
aws s3 sync --exact-timestamps ./benchmark/out/linux/server/ s3://${BUCKET}/assembly/linux/server/
aws s3 sync --exact-timestamps ./benchmark/out/linux/client/ s3://${BUCKET}/assembly/linux/client/
```

## server ops

update binary

```
BUCKET=bench-magiconion-s3-bucket-5c7e45b
dotnet publish benchmark/Benchmark.Server/Benchmark.Server.csproj -c Release -r linux-x64 -p:PublishSingleFile=true --no-self-contained -o benchmark/out/linux/server
aws s3 sync --exact-timestamps ./benchmark/out/linux/server/ s3://${BUCKET}/assembly/linux/server/
instanceId=$(aws ssm describe-instance-information --output json --filters Key=tag-key,Values=bench --filters=Key=PingStatus,Values=Online | jq -r ".InstanceInformationList[].InstanceId")
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/download_server.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations --command-id "${commandId}" --details | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/register_server.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations --command-id "${commandId}" --details | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/stop_server.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations --command-id "${commandId}" --details | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/run_server.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations --command-id "${commandId}" --details | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

get instanceid

```shell
instanceId=$(aws ssm describe-instance-information --output json --filters Key=tag-key,Values=bench --filters=Key=PingStatus,Values=Online | jq -r ".InstanceInformationList[].InstanceId")
```

download

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/download_server.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

register to systemd

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/register_server.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

run

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/run_server.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

logs

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/log_server.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

stop

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/stop_server.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

## client ops

update binary

```
BUCKET=bench-magiconion-s3-bucket-5c7e45b
dotnet publish benchmark/Benchmark.Client/Benchmark.Client.csproj -c Release -r linux-x64 -p:PublishSingleFile=true --no-self-contained -o benchmark/out/linux/client
aws s3 sync --exact-timestamps ./benchmark/out/linux/client/ s3://${BUCKET}/assembly/linux/client/
instanceId=$(aws ssm describe-instance-information --output json --filters Key=tag-key,Values=bench --filters=Key=PingStatus,Values=Online | jq -r ".InstanceInformationList[].InstanceId")
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/download_client.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations --command-id "${commandId}" --details | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/register_client.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations --command-id "${commandId}" --details | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/stop_client.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations --command-id "${commandId}" --details | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/run_client.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations --command-id "${commandId}" --details | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

get instanceid

```shell
# todo add client TagKey
instanceId=$(aws ssm describe-instance-information \
    --output json \
    --filters Key=tag-key,Values=bench \
    --filters Key=PingStatus,Values=Online \
    | jq -r ".InstanceInformationList[].InstanceId")
```

download

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/download_client.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

register to systemd

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/register_client.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

run

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/run_client.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

run (CLI)

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/run_client_cli.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```


logs

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/log_client.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```

stop

```shell
commandId=$(aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/stop_client.json --output json | jq -r ".Command.CommandId")
aws ssm list-command-invocations \
	--command-id "${commandId}" \
	--details \
    | jq -r ".CommandInvocations[].Status, .CommandInvocations[].CommandPlugins[].Output"
```
