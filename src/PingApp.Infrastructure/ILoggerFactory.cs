using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Infrastructure {
    public interface ILoggerFactory {
        ILogger GetLogger<T>();

        ILogger GetLogger(Type type);

        ILogger GetLogger(string name);
    }
}
