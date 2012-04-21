using System;
using System.Collections.Generic;
using PingApp.Entity;

namespace PingApp.Infrastructure {
    public interface IAppParser {
        ISet<int> CollectAppsFromCatalog();

        ISet<int> CollectAppsFromRss(string url);

        ICollection<App> RetrieveApps(ICollection<int> required, int attempts = 0);
    }
}
