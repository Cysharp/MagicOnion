using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MagicOnion.Server;

internal class MagicOnionServerInternalLogger
{
    public static ILogger Current => logger;
        
    static ILogger logger = NullLogger.Instance;

    MagicOnionServerInternalLogger() {}

    public static void SetUnderlyingLogger(ILogger? logger)
    {
        MagicOnionServerInternalLogger.logger = logger ?? NullLogger.Instance;
    }
}
