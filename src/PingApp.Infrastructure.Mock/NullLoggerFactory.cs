using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingApp.Infrastructure.Mock {
    public class NullLoggerFactory : ILoggerFactory {
        private static readonly ILogger instance = new NullLogger();

        public ILogger GetLogger<T>() {
            return instance;
        }

        public ILogger GetLogger(Type type) {
            return instance;
        }

        public ILogger GetLogger(string name) {
            return instance;
        }
    }
}
