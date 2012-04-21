using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Infrastructure {
    public interface ILogger {
        void Log(LogLevel level, string message, params object[] args);

        void LogException(string message, Exception exception);

        void Trace(string message, params object[] args);

        void TraceException(string message, Exception exception);

        void Debug(string message, params object[] args);

        void DebugException(string message, Exception exception);

        void Info(string message, params object[] args);

        void InfoException(string message, Exception exception);

        void Warn(string message, params object[] args);

        void WarnException(string message, Exception exception);

        void Error(string message, params object[] args);

        void ErrorException(string message, Exception exception);

        void Fatal(string message, params object[] args);

        void FatalException(string message, Exception exception);
    }
}
