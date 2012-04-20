using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Infrastructure.Mock {
    public sealed class MockAppParser : IAppParser {
        public ICollection<App> RetrieveApps(ICollection<int> required, int attempts = 0) {
            throw new NotImplementedException();
        }
    }
}
