using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Infrastructure {
    public interface ILoggerFactory {
        ILogger GetLoggerFor<T>();

        ILogger GetLoggerFor<T>(string name);

        ILogger GetLoggerFor(Type type);

        ILogger GetLoggerFor(Type type, string name);
    }
}
