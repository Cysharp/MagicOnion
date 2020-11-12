# MagicOnion.Server.OpenTelemetry

**Supported OpenTelemetry-dotnet version: [0.5.0-beta.2](https://github.com/open-telemetry/opentelemetry-dotnet/releases/tag/0.5.0-beta)**

MagicOnion offer OpenTelemetry support with `MagicOnion.OpenTelemetry` package.
Let's see overview and how to try on localhost.

* overview
* try sample app for OpenTelemetry
* hands on
* try visualization on localhost
* metrics customization
* implement your own metrics

## Overview

MagicOnion.OpenTelemetry is implementation of [open\-telemetry/opentelemetry\-dotnet: OpenTelemetry \.NET SDK](https://github.com/open-telemetry/opentelemetry-dotnet), so you can use any OpenTelemetry exporter, like [Prometheus](https://prometheus.io/), [StackDriver](https://cloud.google.com/stackdriver/pricing), [Zipkin](https://zipkin.io/) and others.

You can collect telemetry and use exporter on MagicOnion Serverside.

## Try sample app for OpenTelemetry

Try OpenTelemetry with ChatApp sample app.

goto [samples/ChatApp](https://github.com/Cysharp/MagicOnion/tree/master/samples/ChatApp) and see README.

## Hands on

What you need to do for Telemetry is followings.

* add reference to the MagicOnion.OpenTelemetry.
* configuration for OpenTelemery.
* configure DI for OpenTelemetry-dotnet.
* (optional) add PrometheusExporterMetricsService for prometheus exporter.
* configure filters/logger for telemetry.
* try your telemetry.

Let's follow the steps. 

### Add reference to the MagicOnion.OpenTelemetry

Add [MagicOnion.OpenTelemetry](https://www.nuget.org/packages/MagicOnion.OpenTelemetry) nuget package to your MagicOnion server project.

```shell
dotnet add package MagicOnion.OpenTelemetry
```

You are ready to configure MagicOnion Filter & Logger for OpenTelemetry.

### Configuration for OpenTelemetry

MagicOnion.OpenTelemetry offers configuration binder.
Default configuration key is `MagicOnion:OpenTelemery`.

* `ServiceName`: Configure Tracer ServiceName
* `MetricsExporterEndpoint`: Configure your metrics exporter's push endpoint. (e.g. Prometheus)
* `TracerExporterEndpoint`: Configure your tracer exporter's push endpoint. (e.g. Zipkin)

```json
{
  "MagicOnion": {
    "OpenTelemetry": {
      "ServiceName": "ChatApp.Server",
      "MetricsExporterEndpoint": "http://127.0.0.1:9184/metrics/",
      "TracerExporterEndpoint": "http://127.0.0.1:9411/api/v2/spans"
    }
  }
}
```

### Configure DI for OpenTelemetry-dotnet

MagicOnion.OpenTelemetry offers extensions for IServiceCollection, `AddMagicOnionOpenTelemetry`.
Register `MagicOnionOpenTelemetryOptions`, `Action<MagicOnionOpenTelemetryMeterFactoryOption>` and `Action<TracerBuilder>` to configure MeterFactory & TracerFactory.

> TIPS: `AddMagicOnionOpenTelemetry` register MagicOnionOpenTelemetryOptions, MeterFactory and TracerFactory as Singleton for you.

```csharp
await MagicOnionHost.CreateDefaultBuilder()
    .UseMagicOnion()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMagicOnionOpenTelemetry((options, meterOptions) =>
        {
            // open-telemetry with Prometheus exporter
            meterOptions.MetricExporter = new PrometheusExporter(new PrometheusExporterOptions() { Url = options.MetricsExporterEndpoint });
        },
        (options, provider, tracerBuilder) =>
        {
            // open-telemetry with Zipkin exporter
            tracerBuilder.AddZipkinExporter(o =>
            {
                o.ServiceName = "MyApp";
                o.Endpoint = new Uri(options.TracerExporterEndpoint);
            });
            // ConsoleExporter will show current tracer activity
            tracerBuilder.AddConsoleExporter();
        });
    })
```


### (Optional) Add PrometheusExporterMetricsService for prometheus exporter.

If you use Prometheus Exporter and require Prometheus Server to recieve pull request from Prometheus Collector Server, see sample IHostedService implementation.

> [PrometheusExporterMetricsService](https://github.com/Cysharp/MagicOnion/blob/master/samples/ChatApp/ChatApp.Server.Telemery/PrometheusExporterMetricsService.cs)
> [PrometheusExporterMetricsHttpServerCustom](https://github.com/Cysharp/MagicOnion/blob/master/samples/ChatApp/ChatApp.Server.Telemery/PrometheusExporterMetricsHttpServerCustom.cs)

```csharp
# Program.cs
.ConfigureServices((hostContext, services) =>
{
    services.AddMagicOnionOpenTelemetry((options, meterOptions) =>
    {
        // your metrics exporter implementation.
        meterOptions.MetricExporter = new PrometheusExporter(new PrometheusExporterOptions() { Url = options.MetricsExporterEndpoint });
    },
    (options, tracerBuilder) =>
    {
        // your tracer exporter implementation.
    });
    // host your prometheus metrics server
    services.AddHostedService<PrometheusExporterMetricsService>();
})
```

### Configure filters/logger for telemetry

You can collect MagicOnion metrics with `MagicOnionFilter`. MagicOnion.OpenTelemetry offers `OpenTelemetryCollectorFilter` and `OpenTelemetryHubCollectorFilter` for you.
You can trace Unary and StreamingHub API by register MagicOnionLogger on each hook point prepared via `IMagicOnionLogger`. MagicOnion.OpenTelemetry offers `OpenTelemetryCollectorLogger` for you.

```csharp
await MagicOnionHost.CreateDefaultBuilder()
    .UseMagicOnion()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMagicOnionOpenTelemetry((options, meterOptions) =>
        {
            // your metrics exporter implementation.
        },
        (options, tracerBuilder) =>
        {
            // your tracer exporter implementation.
        });
    })
    .ConfigureServices((hostContext, services) =>
    {
        var meterFactory = services.BuildServiceProvider().GetService<MeterFactory>();
        services.Configure<MagicOnionHostingOptions>(options =>
        {
            options.Service.GlobalFilters.Add(new OpenTelemetryCollectorFilterFactoryAttribute());
            options.Service.GlobalStreamingHubFilters.Add(new OpenTelemetryHubCollectorFilterFactoryAttribute());
            options.Service.MagicOnionLogger = new OpenTelemetryCollectorLogger(meterProvider);
        });
    })
    .RunConsoleAsync();
```

### Try your telemetry

All implementation is done, let's Debug run MagicOnion and confirm you can see metrics and tracer.

SampleApp `samples/ChatApp.Telemetry/ChatApp.Server` offers sample for Prometheus Metrics exporter and Zipkin Tracer exporter.

Run Zipkin on Docker to recieve tracer from ChatApp.Server.Telemery.

```shell
cd samples/ChatApp.Telemetry
docker-compose -f docker-compose.telemetry.yaml up
```

* Prometheus metrics wlll show on http://localhost:9184/metrics.
* Zipkin tracer will show on http://localhost:9411/zipkin/

Zipkin tracer will be shown as below.

![image](https://user-images.githubusercontent.com/3856350/82529117-3b80d800-9b75-11ea-9e70-4bf15411becc.png)

Prometheus Metrics will be shown as like follows.

```txt
# HELP magiconion_buildservicedefinition_duration_millisecondsMagicOnionmagiconion_buildservicedefinition_duration_milliseconds
# TYPE magiconion_buildservicedefinition_duration_milliseconds summary
magiconion_buildservicedefinition_duration_milliseconds_sum{method="EndBuildServiceDefinition"} 0 1591066746669
magiconion_buildservicedefinition_duration_milliseconds_count{method="EndBuildServiceDefinition"} 0 1591066746669
magiconion_buildservicedefinition_duration_milliseconds{method="EndBuildServiceDefinition",quantile="0"} 1.7976931348623157E+308 1591066746669
magiconion_buildservicedefinition_duration_milliseconds{method="EndBuildServiceDefinition",quantile="1"} -1.7976931348623157E+308 1591066746669
# HELP magiconion_broadcast_request_sizeMagicOnionmagiconion_broadcast_request_size
# TYPE magiconion_broadcast_request_size summary
magiconion_broadcast_request_size_sum{GroupName="SampleRoom"} 0 1591066746669
magiconion_broadcast_request_size_count{GroupName="SampleRoom"} 0 1591066746669
magiconion_broadcast_request_size{GroupName="SampleRoom",quantile="0"} 9.223372036854776E+18 1591066746669
magiconion_broadcast_request_size{GroupName="SampleRoom",quantile="1"} -9.223372036854776E+18 1591066746669
# HELP magiconion_streaminghub_elapsed_millisecondsMagicOnionmagiconion_streaminghub_elapsed_milliseconds
# TYPE magiconion_streaminghub_elapsed_milliseconds summary
magiconion_streaminghub_elapsed_milliseconds_sum{methodType="DuplexStreaming"} 0 1591066746669
magiconion_streaminghub_elapsed_milliseconds_count{methodType="DuplexStreaming"} 0 1591066746669
magiconion_streaminghub_elapsed_milliseconds{methodType="DuplexStreaming",quantile="0"} 1.7976931348623157E+308 1591066746669
magiconion_streaminghub_elapsed_milliseconds{methodType="DuplexStreaming",quantile="1"} -1.7976931348623157E+308 1591066746670
# HELP magiconion_unary_response_sizeMagicOnionmagiconion_unary_response_size
# TYPE magiconion_unary_response_size summary
magiconion_unary_response_size_sum{method="/IChatService/GenerateException"} 0 1591066746669
magiconion_unary_response_size_count{method="/IChatService/GenerateException"} 0 1591066746669
magiconion_unary_response_size{method="/IChatService/GenerateException",quantile="0"} 9.223372036854776E+18 1591066746669
magiconion_unary_response_size{method="/IChatService/GenerateException",quantile="1"} -9.223372036854776E+18 1591066746669
magiconion_unary_response_size_sum{methodType="Unary"} 0 1591066746669
magiconion_unary_response_size_count{methodType="Unary"} 0 1591066746669
magiconion_unary_response_size{methodType="Unary",quantile="0"} 9.223372036854776E+18 1591066746669
magiconion_unary_response_size{methodType="Unary",quantile="1"} -9.223372036854776E+18 1591066746669
```

You may find `MagicOnion/measure/BuildServiceDefinition{MagicOnion_keys_Method="EndBuildServiceDefinition",quantile="0"}` are collected, and other metrics will shown as #HELP.
They will export when Unary/StreamingHub request is comming.

### Tips

* Want insert your own tag to default metrics?

Add defaultTags when register `OpenTelemetryCollectorLogger`.

* Want replace magiconion metrics prefix to my magiconion metrics?

Set metricsPrefix when register `OpenTelemetryCollectorLogger`.
If you pass `yourprefix`, then metrics prefix will change to followings.

```
yourprefix_buildservicedefinition_duration_milliseconds_sum{method="EndBuildServiceDefinition"} 66.7148 1591066185908
```

* Want contain `version` tag to your metrics?

Add version when register `OpenTelemetryCollectorLogger`.

This should output like follows, however current opentelemetry-dotnet Prometheus exporter not respect version tag.

```
magiconion_buildservicedefinition_duration_milliseconds_sum{method="EndBuildServiceDefinition",version="1.0.0"} 66.7148 1591066185908
```

## Implement your own trace

Here's Zipkin Tracer sample with MagicOnion.OpenTelemetry.

![image](https://user-images.githubusercontent.com/3856350/91792704-1a8a5180-ec51-11ea-84d6-b05b201eda7b.png)

Let's see example trace.
MagicOnion.OpenTelemetry automatically trace each StreamingHub and Unary request.

![image](https://user-images.githubusercontent.com/3856350/91793243-9e910900-ec52-11ea-8d2a-a10b6fbc93fe.png)

If you want add your own application trace, use `ActivitySource` which automatically injected by MagicOnion.

![image](https://user-images.githubusercontent.com/3856350/91793396-09424480-ec53-11ea-8c93-a21d1590d06a.png)

Code sample.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    private ActivitySource activitySource;

    public ChatHub(ActivitySource activitySource)
    {
        this.activitySource = activitySource;
    }

    public async Task JoinAsync(JoinRequest request)
    {
        // your logic

        // Trace database operation dummy.
        using (var activity = activitySource.StartActivity("db:room/insert", ActivityKind.Internal))
        {
            // this is sample. use orm or any safe way.
            activity.SetTag("table", "rooms");
            activity.SetTag("query", $"INSERT INTO rooms VALUES (0, '{request.RoomName}', '{request.UserName}', '1');");
            activity.SetTag("parameter.room", request.RoomName);
            activity.SetTag("parameter.username", request.UserName);
            await Task.Delay(TimeSpan.FromMilliseconds(2));
        }
    }
}
```

If you don't want your Trace relates to invoked mehod, use `this.Context.GetTraceContext()` to get your Context's trace directly.

```csharp
// if you don't want set relation to this method, but directly this streaming hub, set hub trace context to your activiy.
var hubTraceContext = this.Context.GetTraceContext();
using (var activity = activitySource.StartActivity("sample:hub_context_relation", ActivityKind.Internal, hubTraceContext))
{
    // this is sample. use orm or any safe way.
    activity.SetTag("message", "this span has no relationship with this method but has with hub context.");
}
```

![image](https://user-images.githubusercontent.com/3856350/91793693-dd738e80-ec53-11ea-8a50-a1fbd6fb4cd0.png)


## Implement your own metrics

Here's Prometheus exporter sample with MagicOnion.OpenTelemetry.

![image](https://user-images.githubusercontent.com/3856350/91793545-72c25300-ec53-11ea-92f6-e1f167054926.png)

Implement `IMagicOnionLogger` to configure your metrics. You can collect metrics when following callbacks are invoked by filter.

```csharp
namespace MagicOnion.Server
{
    public interface IMagicOnionLogger
    {
        void BeginBuildServiceDefinition();
        void BeginInvokeHubMethod(StreamingHubContext context, ArraySegment<byte> request, Type type);
        void BeginInvokeMethod(ServiceContext context, byte[] request, Type type);
        void EndBuildServiceDefinition(double elapsed);
        void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted);
        void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted);
        void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount);
        void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete);
        void WriteToStream(ServiceContext context, byte[] writeData, Type type);
    }
}
```

When implement your own metrics, define `IView` and register it `Stats.ViewManager.RegisterView(YOUR_VIEW);`, then send metrics.

There are several way to send metrics.

> Send each metrics each line.

```csharp
statsRecorder.NewMeasureMap().Put(YOUR_METRICS, 1).Record(TagContext);
```

> Put many metrics and send at once: 

```csharp
var map = statsRecorder.NewMeasureMap(); map.Put(YOUR_METRICS, 1);
map.Put(YOUR_METRICS2, 2);
map.Put(YOUR_METRICS3, 10);
if (isErrorOrInterrupted)
{
    map.Put(YOUR_METRICS4, 3);
}

map.Record(TagContext);
```

> create tag scope and set number of metrics.

```csharp
var tagContextBuilder = Tagger.CurrentBuilder.Put(FrontendKey, TagValue.Create("mobile-ios9.3.5"));
using (var scopedTags = tagContextBuilder.BuildScoped())
{
    StatsRecorder.NewMeasureMap().Put(YOUR_METRICS, 1).Record();
    StatsRecorder.NewMeasureMap().Put(YOUR_METRICS2, 2).Record();
    StatsRecorder.NewMeasureMap().Put(YOUR_METRICS3, 10).Record();
}
```

Make sure your View's column, and metrics TagKey is matched. Otherwise none of metrics will shown.
