using System;
using System.Collections.Generic;

namespace PingApp.Infrastructure {
    public interface ICatalogParser {
        ISet<int> CollectApps();
    }
}
