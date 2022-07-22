using System;
using MagicOnion.Generator.Internal;
using Microsoft.Build.Utilities;

namespace MagicOnion.MSBuild.Tasks
{
    internal class MSBuildMagicOnionGeneratorLogger : IMagicOnionGeneratorLogger
    {
        readonly TaskLoggingHelper log;

        public MSBuildMagicOnionGeneratorLogger(TaskLoggingHelper log)
        {
            this.log = log;
        }

        public void Trace(string message)
            => log.LogMessage(message);

        public void Information(string message)
            => log.LogMessage(message);

        public void Error(string message, Exception exception = null)
        {
            if (exception is null)
            {
                log.LogError(message);
            }
            else
            {
                log.LogErrorFromException(exception);
            }
        }
    }
}