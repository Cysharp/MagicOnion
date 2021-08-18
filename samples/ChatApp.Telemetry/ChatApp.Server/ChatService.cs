using ChatApp.Shared.Services;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grpc.Net.Client;
using MagicOnion.Client;
using MicroServer.Shared;
using MagicOnion.Server.OpenTelemetry;

namespace ChatApp.Server
{
    public class ChatService : ServiceBase<IChatService>, IChatService
    {
        private readonly ActivitySource mysqlActivity = BackendActivitySources.MySQLActivitySource;
        private readonly ActivitySource s2sActivity = BackendActivitySources.S2sActivitySource;
        private readonly MagicOnionOpenTelemetryOptions options;
        private readonly ILogger logger;

        public ChatService(MagicOnionOpenTelemetryOptions options, ILogger<ChatService> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public async UnaryResult<Nil> GenerateException(string message)
        {
            var ex = new System.NotImplementedException();
            // dummy external operation.
            using (var activity = this.mysqlActivity.StartActivity("errors/insert", ActivityKind.Internal))
            {
                // this is sample. use orm or any safe way.
                activity?.SetTag("service.name", options.ServiceName);
                activity?.SetTag("table", "errors");
                activity?.SetTag("query", $"INSERT INTO rooms VALUES ('{ex.Message}', '{ex.StackTrace}');");
                await Task.Delay(TimeSpan.FromMilliseconds(2));
            }
            throw ex;
        }

        public async UnaryResult<Nil> SendReportAsync(string message)
        {
            logger.LogDebug($"{message}");

            // dummy external operation.
            using (var activity = this.mysqlActivity.StartActivity("report/insert", ActivityKind.Internal))
            {
                // this is sample. use orm or any safe way.
                activity?.SetTag("service.name", options.ServiceName);
                activity?.SetTag("table", "report");
                activity?.SetTag("query", $"INSERT INTO report VALUES ('foo', 'bar');");
                await Task.Delay(TimeSpan.FromMilliseconds(2));
            }

            // Server to Server operation
            var channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("Server2ServerEndpoint", EnvironmentVariableTarget.Process) ??  "http://localhost:4999");
            var client = MagicOnionClient.Create<IMessageService>(channel, new[]
            {
                // propagate trace context from ChatApp.Server to MicroServer
                new MagicOnionOpenTelemetryClientFilter(s2sActivity, options),
            });
            await client.SendAsync("hello");

            // dummy external operation.
            using (var activity = this.mysqlActivity.StartActivity("report/get", ActivityKind.Internal))
            {
                // this is sample. use orm or any safe way.
                activity?.SetTag("service.name", options.ServiceName);
                activity?.SetTag("table", "report");
                activity?.SetTag("query", $"INSERT INTO report VALUES ('foo', 'bar');");
                await Task.Delay(TimeSpan.FromMilliseconds(1));
            }

            return Nil.Default;
        }
    }
}
