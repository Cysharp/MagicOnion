using ChatApp.Shared.Services;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace ChatApp.Server
{
    public class ChatService : ServiceBase<IChatService>, IChatService
    {
        private readonly ILogger _logger;

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
        }

        public UnaryResult GenerateException(string message)
        {
            throw new System.NotImplementedException();
        }

        public UnaryResult SendReportAsync(string message)
        {
            _logger.LogDebug($"{message}");

            return UnaryResult.CompletedResult;
        }
    }
}
