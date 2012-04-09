using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using PingApp.Entity;

namespace PingApp.Repository.Mongo {
    public sealed class AppRepository : IAppRepository {
        private readonly MongoCollection<App> apps;

        private readonly MongoCollection<AppUpdate> appUpdates;

        public AppRepository(MongoCollection<App> apps, MongoCollection<AppUpdate> appUpdates) {
            this.apps = apps;
            this.appUpdates = appUpdates;
        }

        public App Retrieve(int id) {
            App app = apps.AsQueryable<App>().First(a => a.Id == id);
            return app;
        }

        public void Save(App app) {
            AppUpdate updateForNew = new AppUpdate() {
                App = app.Id,
                Time = app.Brief.ReleaseDate.Date,
                Type = AppUpdateType.New,
                OldValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol
            };
            appUpdates.Save(updateForNew);
            AppUpdate updateForAdd = new AppUpdate() {
                App = app.Id,
                Time = DateTime.Now,
                Type = AppUpdateType.AddToNote,
                OldValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol
            };
            appUpdates.Save(updateForAdd);
            apps.Save(app);

            app.Brief.LastValidUpdate = updateForAdd;
            apps.Save(app);
        }

        public void Update(App app) {
            apps.Save(app);
        }

        public ISet<int> FindExists(IEnumerable<int> apps) {
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            ICollection<App> result = apps.Find(Query.In("_id", BsonArray.Create(required))).ToArray();
            return result;
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
