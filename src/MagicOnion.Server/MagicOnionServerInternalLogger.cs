using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MagicOnion.Server;

internal class MagicOnionServerInternalLogger
{
    public static ILogger Current => _logger;
        
    static ILogger _logger = NullLogger.Instance;

    private MagicOnionServerInternalLogger() {}

    public static void SetUnderlyingLogger(ILogger? logger)
    {
        _logger = logger ?? NullLogger.Instance;
    }
}
