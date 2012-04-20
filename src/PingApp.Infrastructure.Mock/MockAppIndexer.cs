using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Infrastructure.Mock {
    public sealed class MockAppIndexer : IAppIndexer {
        public void AddApp(App app) {
            throw new NotImplementedException();
        }

        public void DeleteApp(App app) {
            throw new NotImplementedException();
        }

        public void Flush() {
            throw new NotImplementedException();
        }

        public void UpdateApp(App app) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
