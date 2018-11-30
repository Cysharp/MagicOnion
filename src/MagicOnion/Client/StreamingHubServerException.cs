using System;

namespace MagicOnion.Client
{
    public class StreamingHubServerException : Exception
    {
        public StreamingHubServerException(string message)
            : base(message)
        {
        }
    }
}
