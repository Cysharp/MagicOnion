using Grpc.Core.Logging;
using System;

namespace MagicOnion
{
    public class UnityDebugLogger : ILogger
    {
        readonly Type forType;
        readonly string forTypeString;
        readonly bool errorToWarn = true; // default is true(gRPC internal log show to warn)

        public UnityDebugLogger()
            : this(null)
        {
        }

        public UnityDebugLogger(bool errorToWarn)
            : this(null)
        {
            this.errorToWarn = errorToWarn;
        }

        protected UnityDebugLogger(Type forType)
        {
            this.forType = forType;
            if (forType != null)
            {
                var namespaceStr = forType.Namespace ?? "";
                if (namespaceStr.Length > 0)
                {
                    namespaceStr += ".";
                }
                this.forTypeString = namespaceStr + forType.Name + " ";
            }
            else
            {
                this.forTypeString = "";
            }
        }

        /// <summary>
        /// Returns a logger associated with the specified type.
        /// </summary>
        public virtual ILogger ForType<T>()
        {
            if (typeof(T) == forType)
            {
                return this;
            }
            return new UnityDebugLogger(typeof(T));
        }

        /// <summary>Logs a message with severity Debug.</summary>
        public void Debug(string message)
        {
            UnityEngine.Debug.Log(BuildMessage(message));
        }

        /// <summary>Logs a formatted message with severity Debug.</summary>
        public void Debug(string format, params object[] formatArgs)
        {
            UnityEngine.Debug.Log(BuildMessage(format, formatArgs));
        }

        /// <summary>Logs a message with severity Info.</summary>
        public void Info(string message)
        {
            UnityEngine.Debug.Log(BuildMessage(message));
        }

        /// <summary>Logs a formatted message with severity Info.</summary>
        public void Info(string format, params object[] formatArgs)
        {
            UnityEngine.Debug.Log(BuildMessage(format, formatArgs));
        }

        /// <summary>Logs a message with severity Warning.</summary>
        public void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(BuildMessage(message));
        }

        /// <summary>Logs a formatted message with severity Warning.</summary>
        public void Warning(string format, params object[] formatArgs)
        {
            UnityEngine.Debug.LogWarning(BuildMessage(format, formatArgs));
        }

        /// <summary>Logs a message and an associated exception with severity Warning.</summary>
        public void Warning(Exception exception, string message)
        {
            Warning(message + " " + exception);
        }

        /// <summary>Logs a message with severity Error.</summary>
        public void Error(string message)
        {
            if (errorToWarn)
            {
                UnityEngine.Debug.LogWarning(BuildMessage(message));
            }
            else
            {
                UnityEngine.Debug.LogError(BuildMessage(message));
            }
        }

        /// <summary>Logs a formatted message with severity Error.</summary>
        public void Error(string format, params object[] formatArgs)
        {
            if (errorToWarn)
            {
                UnityEngine.Debug.LogWarning(BuildMessage(format, formatArgs));
            }
            else
            {
                UnityEngine.Debug.LogError(BuildMessage(format, formatArgs));
            }
        }

        /// <summary>Logs a message and an associated exception with severity Error.</summary>
        public void Error(Exception exception, string message)
        {
            Error(message + " " + exception);
        }

        /// <summary>Gets the type associated with this logger.</summary>
        protected Type AssociatedType
        {
            get { return forType; }
        }

        string BuildMessage(string message)
        {
            if (forType != null)
            {
                return forTypeString + message;
            }
            else
            {
                return message;
            }
        }

        string BuildMessage(string format, object[] args)
        {
            if (forType != null)
            {
                return forTypeString + string.Format(format, args);
            }
            else
            {
                return string.Format(format, args);
            }
        }
    }
}