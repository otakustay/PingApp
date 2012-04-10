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

        private readonly MongoCollection<RevokedApp> revokedApps;

        private readonly MongoCollection<AppUpdate> appUpdates;


        public AppRepository(MongoCollection<App> apps,
            MongoCollection<RevokedApp> revokedApps, MongoCollection<AppUpdate> appUpdates) {
            this.apps = apps;
            this.revokedApps = revokedApps;
            this.appUpdates = appUpdates;
        }

        public App Retrieve(int id) {
            App app = apps.AsQueryable<App>().First(a => a.Id == id);
            return app;
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            ICollection<App> result = apps.Find(Query.In("_id", BsonArray.Create(required))).ToArray();
            return result;
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
                Type = AppUpdateType.AddToPing,
                OldValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol
            };
            appUpdates.Save(updateForAdd);

            app.Brief.LastValidUpdate = updateForAdd;
            apps.Save(app);
        }

        public void Update(App app) {
            apps.Save(app);
        }

        public ICollection<App> Retrieve(int offset, int limit) {
            ICollection<App> result = apps.FindAll()
                .SetSkip(offset)
                .SetLimit(limit)
                .ToArray();
            return result;
        }

        public RevokedApp Revoke(App app) {
            // 添加下架信息
            AppUpdate update = new AppUpdate() {
                App = app.Id,
                Type = AppUpdateType.Revoke,
                OldValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol,
                Time = DateTime.Now
            };
            appUpdates.Save(update);

            // 从在售应用中移除
            apps.Remove(Query.EQ("_id", app.Id), RemoveFlags.Single);

            // 添加到下架应用中
            RevokedApp revoked = new RevokedApp(app);
            revokedApps.Save(revoked);

            return revoked;
        }

        public ICollection<RevokedApp> RetrieveRevoked(int offset, int limit) {
            ICollection<RevokedApp> result = revokedApps.FindAll()
                .SetSkip(offset)
                .SetLimit(limit)
                .ToArray();
            return result;
        }

        public void Resurrect(App app) {
            // 添加重新上架信息
            AppUpdate update = new AppUpdate() {
                App = app.Id,
                NewValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol,
                Time = DateTime.Now,
                Type = AppUpdateType.Resurrect
            };
            appUpdates.Save(update);

            // 从下架应用中移除
            revokedApps.Remove(Query.EQ("_id", app.Id));

            // 重新加入到应用集合中，并标记最后更新
            app.Brief.LastValidUpdate = update;
            apps.Save(app);
        }
    }
}
