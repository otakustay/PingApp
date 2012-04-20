using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingApp.Infrastructure.Mock {
    public sealed class MockCatalogParser : ICatalogParser {
        public ISet<int> CollectApps() {
            throw new NotImplementedException();
        }
    }
}
