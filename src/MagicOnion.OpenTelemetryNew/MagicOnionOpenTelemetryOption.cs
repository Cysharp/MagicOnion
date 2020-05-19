using System.Reflection;

namespace MagicOnion.OpenTelemetry
{
    public class MagicOnionOpenTelemetryOption
    {
        /// <summary>
        /// Service Name for the app. default is Assembly name.
        /// </summary>
        public string ServiceName { get; }

        public MagicOnionOpenTelemetryOption()
        {
            ServiceName = Assembly.GetExecutingAssembly().GetName().Name;
        }

        public MagicOnionOpenTelemetryOption(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}