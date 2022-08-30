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