using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Infrastructure.Mock {
    public sealed class MockUpdateNotifier : IUpdateNotifier {
        public void ProcessUpdate(App app, AppUpdate update) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
