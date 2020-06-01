using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Metrics.Configuration;
using OpenTelemetry.Trace;

namespace MagicOnion.OpenTelemetry
{
    /// <summary>
    /// Collect OpemTelemetry Meter Metrics.
    /// </summary>
    public class OpenTelemetryCollectorLogger : IMagicOnionLogger
    {
        static readonly string MethodKey = "method";
        static readonly string MethodTypeKey = "methodType";
        readonly IEnumerable<KeyValuePair<string, string>> defaultLabels;
        readonly ConcurrentDictionary<string, HashSet<KeyValuePair<string, string>>> labelCache = new ConcurrentDictionary<string, HashSet<KeyValuePair<string, string>>>();
        readonly ConcurrentDictionary<string, HashSet<KeyValuePair<string, string>>> broadcastLabelCache = new ConcurrentDictionary<string, HashSet<KeyValuePair<string, string>>>();

        readonly MeasureMetric<double> buildServiceDefinitionMeasure;
        readonly CounterMetric<long> unaryRequestCounter;
        readonly MeasureMetric<long> unaryResponseSizeMeasure;
        readonly CounterMetric<long> unaryErrorCounter;
        readonly MeasureMetric<double> unaryElapsedMeasure;

        readonly CounterMetric<long> streamingHubErrorCounter;
        readonly MeasureMetric<double> streamingHubElapsedMeasure;
        readonly CounterMetric<long> streamingHubRequestCounter;
        readonly MeasureMetric<long> streamingHubResponseSizeMeasure;
        readonly CounterMetric<long> streamingHubConnectCounter;
        readonly CounterMetric<long> streamingHubDisconnectCounter;

        readonly CounterMetric<long> broadcastRequestCounter;
        readonly MeasureMetric<long> broadcastRequestSizeMeasure;
        readonly CounterMetric<long> broadcastGroupCounter;

        public OpenTelemetryCollectorLogger(MeterFactory meterFactory, string metricsPrefix = "magiconion", string version = null, IEnumerable<KeyValuePair<string, string>> defaultLabels = null)
        {
            if (meterFactory == null) throw new ArgumentNullException(nameof(meterFactory));

            // configure defaultTags included as default tag
            this.defaultLabels = defaultLabels ?? Array.Empty<KeyValuePair<string, string>>();

            // todo: how to description?
            var meter = meterFactory.GetMeter("MagicOnion", version);

            // Service build time. ms
            buildServiceDefinitionMeasure = meter.CreateDoubleMeasure($"{metricsPrefix}_buildservicedefinition_duration_milliseconds"); // sum

            // Unary request count. num
            unaryRequestCounter = meter.CreateInt64Counter($"{metricsPrefix}_unary_requests_count"); // sum
            // Unary API response size. bytes
            unaryResponseSizeMeasure = meter.CreateInt64Measure($"{metricsPrefix}_unary_response_size"); // sum
            // Unary API error Count. num
            unaryErrorCounter = meter.CreateInt64Counter($"{metricsPrefix}_unary_error_count"); // sum
            // Unary API elapsed time. ms
            unaryElapsedMeasure = meter.CreateDoubleMeasure($"{metricsPrefix}_unary_elapsed_milliseconds"); // sum

            // StreamingHub API error Count. num
            streamingHubErrorCounter = meter.CreateInt64Counter($"{metricsPrefix}_streaminghub_error_count"); // sum
            // StreamingHub API elapsed time. ms
            streamingHubElapsedMeasure = meter.CreateDoubleMeasure($"{metricsPrefix}_streaminghub_elapsed_milliseconds"); // sum
            // StreamingHub request count. num
            streamingHubRequestCounter = meter.CreateInt64Counter($"{metricsPrefix}_streaminghub_requests_count"); // sum
            // StreamingHub API response size. bytes
            streamingHubResponseSizeMeasure = meter.CreateInt64Measure($"{metricsPrefix}_streaminghub_response_size"); // sum
            // ConnectCount - DisconnectCount = current connect count. (successfully disconnected)
            // StreamingHub connect count. num
            streamingHubConnectCounter = meter.CreateInt64Counter($"{metricsPrefix}_streaminghub_connect_count"); // sum
            // StreamingHub disconnect count. num
            streamingHubDisconnectCounter = meter.CreateInt64Counter($"{metricsPrefix}_streaminghub_disconnect_count"); // sum

            // HubBroadcast request count. num
            broadcastRequestCounter = meter.CreateInt64Counter($"{metricsPrefix}_broadcast_requests_count"); // sum
            // HubBroadcast request size. num
            broadcastRequestSizeMeasure = meter.CreateInt64Measure($"{metricsPrefix}_broadcast_request_size"); // sum
            // HubBroadcast group count. num
            broadcastGroupCounter = meter.CreateInt64Counter($"{metricsPrefix}_broadcast_group_count"); // sum
        }

        IEnumerable<KeyValuePair<string, string>> CreateLabel(ServiceContext context)
        {
            // Unary start from /{UnaryInterface}/{Method}
            var value = context.CallContext.Method;
            var label = labelCache.GetOrAdd(value, new HashSet<KeyValuePair<string, string>>(defaultLabels)
            {
                new KeyValuePair<string, string>( MethodKey, context.CallContext.Method),
            });
            label.Add(new KeyValuePair<string, string>(MethodTypeKey, MethodTypeToString(context.MethodType)));
            return label;
        }
        IEnumerable<KeyValuePair<string, string>> CreateLabel(StreamingHubContext context)
        {
            // StreamingHub start from {HubInterface}/{Method}
            var value = "/" + context.Path;
            var label = labelCache.GetOrAdd(value, new HashSet<KeyValuePair<string, string>>(defaultLabels)
            {
                new KeyValuePair<string, string>( MethodKey, value),
            });
            label.Add(new KeyValuePair<string, string>(MethodTypeKey, MethodTypeToString(context.ServiceContext.MethodType)));
            return label;
        }
        IEnumerable<KeyValuePair<string, string>> CreateLabel(string value)
        {
            var label = labelCache.GetOrAdd(value, new HashSet<KeyValuePair<string, string>>(defaultLabels)
            {
                new KeyValuePair<string, string>( MethodKey, value),
            });
            return label;
        }
        IEnumerable<KeyValuePair<string, string>> CreateBroadcastLabel(string value)
        {
            var label = broadcastLabelCache.GetOrAdd(value, new HashSet<KeyValuePair<string, string>>(defaultLabels)
            {
                new KeyValuePair<string, string>( "GroupName", value),
            });
            return label;
        }

        public void BeginBuildServiceDefinition()
        {
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
            buildServiceDefinitionMeasure.Record(default(SpanContext), elapsed, CreateLabel(nameof(EndBuildServiceDefinition)));
        }

        public void BeginInvokeMethod(ServiceContext context, byte[] request, Type type)
        {
            if (context.MethodType == MethodType.DuplexStreaming && context.CallContext.Method.EndsWith("/Connect"))
            {
                streamingHubConnectCounter.Add(default(SpanContext), 1, CreateLabel(context));
            }
            else if (context.MethodType == MethodType.Unary)
            {
                unaryRequestCounter.Add(default(SpanContext), 1, CreateLabel(context));
            }
        }

        public void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            if (context.MethodType == MethodType.DuplexStreaming && context.CallContext.Method.EndsWith("/Connect"))
            {
                streamingHubDisconnectCounter.Add(default(SpanContext), 1, CreateLabel(context));
            }
            else if (context.MethodType == MethodType.Unary)
            {
                var spanContext = default(SpanContext);
                var label = CreateLabel(context);
                unaryElapsedMeasure.Record(spanContext, elapsed, label);
                unaryResponseSizeMeasure.Record(spanContext, response.LongLength, label);
                if (isErrorOrInterrupted)
                {
                    unaryErrorCounter.Add(spanContext, 1, label);
                }
            }
        }

        public void BeginInvokeHubMethod(StreamingHubContext context, ReadOnlyMemory<byte> request, Type type)
        {
            streamingHubRequestCounter.Add(default(SpanContext), 1, CreateLabel(context));
        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var spanContext = default(SpanContext);
            var label = CreateLabel(context);
            streamingHubElapsedMeasure.Record(spanContext, elapsed, label);
            streamingHubRequestCounter.Add(spanContext, responseSize, label);
            if (isErrorOrInterrupted)
            {
                streamingHubErrorCounter.Add(spanContext, 1, label);
            }
        }

        public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
        {
            var spanContext = default(SpanContext);
            broadcastRequestCounter.Add(spanContext, 1, CreateBroadcastLabel(groupName));
            broadcastGroupCounter.Add(spanContext, broadcastGroupCount, CreateBroadcastLabel(groupName));
            broadcastRequestSizeMeasure.Record(spanContext, responseSize, CreateBroadcastLabel(groupName));
        }

        public void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete)
        {
        }

        public void WriteToStream(ServiceContext context, byte[] writeData, Type type)
        {
        }

        string MethodTypeToString(MethodType type)
        {
            switch (type)
            {
                case MethodType.Unary:
                    return "Unary";
                case MethodType.ClientStreaming:
                    return "ClientStreaming";
                case MethodType.ServerStreaming:
                    return "ServerStreaming";
                case MethodType.DuplexStreaming:
                    return "DuplexStreaming";
                default:
                    return ((int)type).ToString();
            }
        }
    }
}