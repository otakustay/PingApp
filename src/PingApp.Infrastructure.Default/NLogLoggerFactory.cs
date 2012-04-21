using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace PingApp.Infrastructure.Default {
    public class NLogLoggerFactory : ILoggerFactory {
        private const string DEFAULT_LOGGER_NAME = "default";

        public ILogger GetLogger<T>() {
            return GetLogger(typeof(T).FullName);
        }

        public ILogger GetLogger(Type type) {
            return GetLogger(type.FullName);
        }

        public ILogger GetLogger(string name) {
            Logger logger = LogManager.GetLogger(name);
            return new NLogAdapter(logger);
        }
    }
}
