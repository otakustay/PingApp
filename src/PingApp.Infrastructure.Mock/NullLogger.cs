using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingApp.Infrastructure.Mock {
    sealed class NullLogger : ILogger {
        public void Log(LogLevel level, string message, params object[] args) {
        }

        public void LogException(LogLevel level, string message, Exception exception) {
        }

        public void Trace(string message, params object[] args) {
        }

        public void TraceException(string message, Exception exception) {
        }

        public void Debug(string message, params object[] args) {
        }

        public void DebugException(string message, Exception exception) {
        }

        public void Info(string message, params object[] args) {
            throw new NotImplementedException();
        }

        public void InfoException(string message, Exception exception) {
        }

        public void Warn(string message, params object[] args) {
        }

        public void WarnException(string message, Exception exception) {
        }

        public void Error(string message, params object[] args) {
        }

        public void ErrorException(string message, Exception exception) {
        }

        public void Fatal(string message, params object[] args) {
        }

        public void FatalException(string message, Exception exception) {
        }
    }
}
