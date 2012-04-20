using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Infrastructure.Mock {
    public sealed class MockAppIndexer : IAppIndexer {
        private readonly List<App> addQueue = new List<App>();

        private readonly List<App> updateQueue = new List<App>();

        private readonly List<App> deleteQueue = new List<App>();

        public List<App> Added { get; private set; }

        public List<App> Updated { get; private set; }

        public List<App> Deleted { get; private set; }

        public MockAppIndexer() {
            Added = new List<App>();
            Updated = new List<App>();
            Deleted = new List<App>();
        }

        public void AddApp(App app) {
            lock (addQueue) {
                addQueue.Add(app);
            }
        }

        public void DeleteApp(App app) {
            lock (deleteQueue) {
                deleteQueue.Add(app);
            }
        }

        public void UpdateApp(App app) {
            lock (updateQueue) {
                updateQueue.Add(app);
            }
        }

        public void Flush() {
            lock (addQueue) {
                Added.AddRange(addQueue);
                addQueue.Clear();
            }
            lock (updateQueue) {
                Updated.AddRange(updateQueue);
                updateQueue.Clear();
            }
            lock (deleteQueue) {
                Deleted.AddRange(deleteQueue);
                deleteQueue.Clear();
            }
        }

        public void Dispose() {
        }
    }
}
