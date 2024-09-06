using NLog;
using System;

namespace Components
{
    public interface ILog
    {
        void Information(string message);
        void Warning(string message);
        void Debug(string message);
        void Error(string message);
        void Log(LogLevel level, string message, Exception exception = null);
    }

    public class LogNLog : ILog
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        public LogNLog()
        {
        }

        public void Information(string message)
        {
            logger.Info(message);
        }

        public void Warning(string message)
        {
            logger.Warn(message);
        }

        public void Debug(string message)
        {
            logger.Debug(message);
        }

        public void Error(string message)
        {
            logger.Error(message);
        }

        public void Log(LogLevel level, string message, Exception exception = null)
        {
            logger.Log(level, exception, message);
        }
    }
}
