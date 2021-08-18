using System;
using System.Reflection;

namespace MagicOnion.Server.OpenTelemetry.Internal
{
    internal static class MagicOnionInstrumentation
    {
        /// <summary>
        /// The assembly name.
        /// </summary>
        internal static readonly AssemblyName AssemblyName = typeof(MagicOnionInstrumentation).Assembly.GetName();

        /// <summary>
        /// The activity source name.
        /// </summary>
        internal static readonly string ActivitySourceName = AssemblyName.Name;

        /// <summary>
        /// The version.
        /// </summary>
        internal static readonly Version Version = AssemblyName.Version;
    }
}
