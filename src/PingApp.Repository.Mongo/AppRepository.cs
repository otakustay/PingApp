using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PingApp.Entity;

namespace PingApp.Repository.Mongo {
    public sealed class AppRepository : IAppRepository {
        private readonly MongoCollection<App> apps;

        public AppRepository(MongoCollection<App> apps) {
            this.apps = apps;
        }

        public App Retrieve(int id) {
            App app = apps.AsQueryable<App>().First(a => a.Id == id);
            return app;
        }

        public void Save(App app) {
            apps.Save(app);
        }

        public void Update(App app) {
            apps.Save(app);
        }

        public ISet<int> FindExists(IEnumerable<int> apps) {
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(int offset, int limit) {
            ICollection<App> result = apps.AsQueryable<App>().Skip(offset).Take(limit).ToArray();
            return result;
        }

        public ICollection<int> RetrieveIdentities(int offset, int limit) {
            throw new NotImplementedException();
        }

        public IDictionary<int, string> RetrieveHash(int offset, int limit) {
            throw new NotImplementedException();
        }

        public IDictionary<int, string> RetrieveHash(IEnumerable<int> apps) {
            throw new NotImplementedException();
        }
    }
}
