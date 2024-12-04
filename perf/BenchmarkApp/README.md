# Benchmark: PerformanceTest

## Usage
### Server
```
dotnet run -c Release --
```

### Client
```
$ dotnet run -c Release -- -u http://localhost:5000 -s unary
```

#### Run all scenarios (`-s All`)
```
$ dotnet run -c Release -- -u http://localhost:5000 -s All
```

#### Run scenario repeatedly (`-r`)
```
$ dotnet run -c Release -- -u http://localhost:5000 -s unary -r 5
```

#### Write a report to a text file (`--report`)
```
$ dotnet run -c Release -- -u http://localhost:5000 -s unary --report report.txt
```

#### Use Repository-local MagicOnion.Client (`dotnet` command option)
```
$ dotnet run -c Release -p:UseRepositoryClient=true -- -u http://localhost:5000 -s unary
```

## Datadog Event

Post Datadog event if you changed TargetFramework.

```shell
curl -X POST "https://api.datadoghq.com/api/v1/events" \
-H "Accept: application/json" \
-H "Content-Type: application/json" \
-H "DD-API-KEY: ${DD_API_KEY}" \
-H "DD-APPLICATION-KEY: ${DD_APP_KEY}" \
-d @- << EOF
{
  "tags": [
    "type:benchmark",
    "app:magiconion",
    "changed:target-framework"
  ],
  "title": "magiconion target framework updated",
  "text": "magiconion target framework version has been updated from 8.0 to 9.0"
}
EOF
```
