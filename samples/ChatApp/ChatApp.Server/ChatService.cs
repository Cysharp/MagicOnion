using ChatApp.Shared.Services;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace ChatApp.Server;

public class ChatService : ServiceBase<IChatService>, IChatService
{
    private readonly ILogger logger;

    public ChatService(ILogger<ChatService> logger)
    {
        this.logger = logger;
    }

    public UnaryResult GenerateException(string message)
    {
        throw new System.NotImplementedException();
    }

    public UnaryResult SendReportAsync(string message)
    {
        logger.LogDebug($"{message}");

        return UnaryResult.CompletedResult;
    }
}
