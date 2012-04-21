using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace PingApp.Infrastructure.Default {
    public sealed class NLogAdapter : ILogger {
        private static Dictionary<LogLevel, NLog.LogLevel> logLevelMapping = new Dictionary<LogLevel, NLog.LogLevel>() {
            { LogLevel.Trace, NLog.LogLevel.Trace },
            { LogLevel.Debug, NLog.LogLevel.Debug },
            { LogLevel.Info, NLog.LogLevel.Info },
            { LogLevel.Warn, NLog.LogLevel.Warn },
            { LogLevel.Error, NLog.LogLevel.Error },
            { LogLevel.Fatal, NLog.LogLevel.Fatal },
        };

        private readonly Logger logger;

        public NLogAdapter(Logger logger) {
            this.logger = logger;
        }

        public void Log(LogLevel level, string message, params object[] args) {
            NLog.LogLevel logLevel = logLevelMapping[level];
            logger.Log(logLevel, message, args);
        }

        public void LogException(LogLevel level, string message, Exception exception) {
            NLog.LogLevel logLevel = logLevelMapping[level];
            logger.LogException(logLevel, message, exception);
        }

        public void Trace(string message, params object[] args) {
            logger.Trace(message, args);
        }

        public void TraceException(string message, Exception exception) {
            logger.TraceException(message, exception);
        }

        public void Debug(string message, params object[] args) {
            logger.Debug(message, args);
        }

        public void DebugException(string message, Exception exception) {
            logger.DebugException(message, exception);
        }

        public void Info(string message, params object[] args) {
            logger.Info(message, args);
        }

        public void InfoException(string message, Exception exception) {
            logger.InfoException(message, exception);
        }

        public void Warn(string message, params object[] args) {
            logger.Warn(message, args);
        }

        public void WarnException(string message, Exception exception) {
            logger.WarnException(message, exception);
        }

        public void Error(string message, params object[] args) {
            logger.Error(message, args);
        }

        public void ErrorException(string message, Exception exception) {
            logger.ErrorException(message, exception);
        }

        public void Fatal(string message, params object[] args) {
            logger.Fatal(message, args);
        }

        public void FatalException(string message, Exception exception) {
            logger.FatalException(message, exception);
        }
    }
}
