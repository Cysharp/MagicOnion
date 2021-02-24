using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmark.Server
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly HelloReply _simpleReply;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
            // cached response as just benchmark
            _simpleReply = new HelloReply { Message = true };
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(_simpleReply);
        }
    }
}
