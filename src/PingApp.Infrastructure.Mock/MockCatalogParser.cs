using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingApp.Infrastructure.Mock {
    public sealed class MockCatalogParser : ICatalogParser {
        private IEnumerable<int> result;

        public MockCatalogParser(IEnumerable<int> result) {
            this.result = result;
        }

        public MockCatalogParser(int start = 0, int count = 10) {
            this.result = Enumerable.Range(start, count);
        }

        public ISet<int> CollectApps() {
            return new HashSet<int>(result);
        }
    }
}
