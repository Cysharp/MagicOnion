using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.Server
{
    public static class Statistics
    {
        // todo: show statistics in interval
        public static int TotalConnections => hubConnections + unaryConnections;
        public static int HubConnections => hubConnections;
        public static int HubErrors => hubErrors;
        public static int UnaryConnections => unaryConnections;
        public static int UnaryErrors => unaryErrors;

        private static int hubConnections = 0;
        private static int hubErrors = 0;
        private static int unaryConnections = 0;
        private static int unaryErrors = 0;

        public static void UnaryConnected()
        {
            Interlocked.Increment(ref unaryConnections);
        }
        public static void UnaryDisconnected()
        {
            Interlocked.Decrement(ref unaryConnections);
        }
        public static void UnaryError()
        {
            Interlocked.Increment(ref unaryErrors);
        }

        public static void HubConnected()
        {
            Interlocked.Increment(ref hubConnections);
        }
        public static void HubDisconnected()
        {
            Interlocked.Decrement(ref hubConnections);
        }
        public static void HubError()
        {
            Interlocked.Increment(ref hubErrors);
        }
    }
}
