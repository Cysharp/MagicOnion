#if !GRPC_DOTNET
using System;
using Grpc.Core.Logging;



namespace MagicOnion.Server
{
    /// <summary>
    /// Provides the ILogger to composite multiple loggers.
    /// </summary>
    public class CompositeLogger : ILogger
    {
        #region Properties
        /// <summary>
        /// Gets a collection of ILogger.
        /// </summary>
        private ILogger[] Loggers { get; }


        /// <summary>
        /// Gets a type for logger.
        /// </summary>
        private Type Type { get; }
        #endregion


        #region Constructors
        /// <summary>
        /// Creates instance.
        /// </summary>
        /// <param name="loggers">Composited loggers.</param>
        public CompositeLogger(params ILogger[] loggers)
            : this(loggers, null)
        {}


        /// <summary>
        /// Creates instance.
        /// </summary>
        /// <param name="loggers">Composited loggers.</param>
        /// <param name="type">Type for logger.</param>
        private CompositeLogger(ILogger[] loggers, Type type)
        {
            if (loggers == null)
                throw new ArgumentNullException(nameof(loggers));

            this.Loggers = loggers;
            this.Type = type;
        }
        #endregion


        #region ILogger implementations
        /// <summary>
        /// Gets the ILogger instance for target type.
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <returns>ILogger instance.</returns>
        public ILogger ForType<T>()
        {
            if (this.Type == typeof(T))
                return this;

            var loggers = new ILogger[this.Loggers.Length];
            for (var i = 0; i < loggers.Length; i++)
                loggers[i] = this.Loggers[i].ForType<T>();

            return new CompositeLogger(loggers, typeof(T));
        }


        /// <summary>
        /// Output debug message.
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Debug(message);
        }


        /// <summary>
        /// Output debug message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Debug(string format, params object[] args)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Debug(format, args);
        }


        /// <summary>
        /// Output information message.
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Info(message);
        }


        /// <summary>
        /// Output information message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Info(string format, params object[] args)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Info(format, args);
        }


        /// <summary>
        /// Output warning message.
        /// </summary>
        /// <param name="message"></param>
        public void Warning(string message)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Warning(message);
        }


        /// <summary>
        /// Output warning message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Warning(string format, params object[] args)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Warning(format, args);
        }


        /// <summary>
        /// Output warning message.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        public void Warning(Exception exception, string message)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Warning(exception, message);
        }


        /// <summary>
        /// Output error message.
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Error(message);
        }


        /// <summary>
        /// Output error message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Error(string format, params object[] args)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Error(format, args);
        }


        /// <summary>
        /// Output error message.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        public void Error(Exception exception, string message)
        {
            for (var i = 0; i < this.Loggers.Length; i++)
                this.Loggers[i].Error(exception, message);
        }
        #endregion
    }
}
#endif