using System;
using System.Collections.Generic;
using System.Text;

namespace MagicOnion.Client
{
    public interface IMagicOnionClientLogger
    {
        void Error(Exception ex, string message);
        void Information(string message);
        void Debug(string message);
        void Trace(string message);
    }

    public sealed class NullMagicOnionClientLogger : IMagicOnionClientLogger
    {
        public static IMagicOnionClientLogger Instance { get; } = new NullMagicOnionClientLogger();
        private NullMagicOnionClientLogger() {}
        public void Error(Exception ex, string message)
        {
        }

        public void Information(string message)
        {
        }

        public void Debug(string message)
        {
        }

        public void Trace(string message)
        {
        }
    }
}
