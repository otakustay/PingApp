using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Infrastructure.Mock {
    public sealed class MockUpdateNotifier : IUpdateNotifier {
        public Dictionary<App, List<AppUpdate>> UpdateData { get; private set; }

        public MockUpdateNotifier() {
            UpdateData = new Dictionary<App, List<AppUpdate>>();
        }

        public void ProcessUpdate(App app, AppUpdate update) {
            lock (UpdateData) {
                if (!UpdateData.ContainsKey(app)) {
                    UpdateData[app] = new List<AppUpdate>();
                }
                UpdateData[app].Add(update);
            }
        }

        public void Dispose() {
        }
    }
}
