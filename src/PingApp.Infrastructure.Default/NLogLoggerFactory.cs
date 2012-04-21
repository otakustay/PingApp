using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace PingApp.Infrastructure.Default {
    public class NLogLoggerFactory : ILoggerFactory {
        private const string DEFAULT_LOGGER_NAME = "default";

        public ILogger GetLoggerFor<T>() {
            return GetLoggerFor<T>(DEFAULT_LOGGER_NAME);
        }

        public ILogger GetLoggerFor<T>(string name) {
            return GetLoggerFor(typeof(T), name);
        }

        public ILogger GetLoggerFor(Type type) {
            return GetLoggerFor(type, DEFAULT_LOGGER_NAME);
        }

        public ILogger GetLoggerFor(Type type, string name) {
            Logger logger = LogManager.GetLogger(name, type);
            return new NLogAdapter(logger);
        }
    }
}
