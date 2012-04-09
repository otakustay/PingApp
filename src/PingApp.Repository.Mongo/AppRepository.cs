using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using PingApp.Entity;

namespace PingApp.Repository.Mongo {
    public sealed class AppRepository : IAppRepository {
        private readonly MongoCollection<App> apps;

        private readonly MongoCollection<RevokedApp> revokedApps;

        public AppRepository(MongoCollection<App> apps, MongoCollection<RevokedApp> revokedApps) {
            this.apps = apps;
            this.revokedApps = revokedApps;
        }

        public App Retrieve(int id) {
            App app = apps.AsQueryable<App>().First(a => a.Id == id);
            return app;
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            throw new NotImplementedException();
        }

        public void Save(App app) {
            apps.Save(app);
        }

        public void Update(App app) {
            apps.Save(app);
        }

        public ICollection<App> Retrieve(int offset, int limit) {
            ICollection<App> result = apps.AsQueryable<App>().Skip(offset).Take(limit).ToArray();
            return result;
        }

        public RevokedApp Revoke(App app) {
            apps.Remove(Query.EQ("_id", app.Id), RemoveFlags.Single);

            RevokedApp revoked = new RevokedApp(app);
            revokedApps.Save(revoked);

            return revoked;
        }
    }
}
