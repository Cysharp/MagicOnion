using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicOnion
{
    public static class ChannelExtensions
    {
        /// <summary>
        /// Safety way for subscribe streaming events.
        /// </summary>
        public static void RegisterStreamingSubscription(this Channel channel, IDisposable subscription)
        {
            channel.ShutdownToken.Register(() => subscription.Dispose());
        }
    }
}
