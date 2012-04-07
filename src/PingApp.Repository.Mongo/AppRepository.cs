using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using PingApp.Entity;
using FluentMongo.Linq;

namespace PingApp.Repository.Mongo {
    public sealed class AppRepository : IAppRepository {
        private readonly MongoCollection<App> apps;

        public AppRepository(MongoCollection<App> apps) {
            this.apps = apps;
        }

        public App Retrieve(int id) {
            App app = apps.AsQueryable().First(a => a.Id == id);
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
